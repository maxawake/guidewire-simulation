import argparse
import os
import sys
import itk
import vtk
import packaging.version



def extract_connected_components(input_file, output_folder):
    """
    The input_file and the output_folder are the two arguments from the terminal and are processed via the python argparse function. The input_file holds the file that one wants to process (smooth, divide components, get mesh from the volume data). The output_folder is the folder that the processed files are saved to.
    If this python file and the input file or the output folder are not in the same directory, it is necessary to use the full file path.

    The first goal is to divide up all components into their own files. 
    Here it might happen, that one input file generates many components, as sometimes volume data has some very small components that can go unnoticed, but count as individual #components. 

    Parameters
    ----------
    input_file : str
        The path to the input file that should be processed.
    output_folder : str
        The path to the output folder where the processed files should be saved.
    """
    
    #Load the input image that was input in the command line
    input_image = itk.imread(input_file)

    #this itk filter labels each component.
    connected_component_filter = itk.ConnectedComponentImageFilter.New(input_image)
    connected_component_filter.Update()

    # Get the number of components
    number_of_components = connected_component_filter.GetObjectCount()

    # Create the output folder
    os.makedirs(output_folder, exist_ok=True)

    #This saves each component as a separate file. In the following all components will be smoothed individually.
    component_files = []
    for component_id in range(1, number_of_components + 1):
        # Create a binary image of the current component
        component_image = itk.BinaryThresholdImageFilter.New(connected_component_filter.GetOutput())
        component_image.SetLowerThreshold(component_id)
        component_image.SetUpperThreshold(component_id)
        component_image.SetInsideValue(1)
        component_image.SetOutsideValue(0)
        component_image.Update()

        #Now the components are saved as a separate files
        component_file = os.path.join(output_folder, f"Component{component_id}.mha")
        itk.imwrite(component_image.GetOutput(), component_file)
        component_files.append(component_file)
        #Here some feedback is given to see if running the command is working.
        print(f"Component {component_id} saved as {component_file}")

    return component_files

#now the #for this I used a code snippet of itk.examples (link: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!)
def smooth_components(component_files, output_folder, sigma):
    """Different components in each file are getting smoothed (as volume data). The sigma value is the standard deviation of the Gaussian kernel that is used for smoothing.

    Parameters
    ----------
    component_files : str
        The path to the input file that should be processed.
    output_folder : str
        The path to the output folder where the processed files should be saved.
    sigma : float
        The standard deviation of the Gaussian kernel used for smoothing.
    """
    for component_file in component_files:
        output_file = os.path.join(output_folder, f"{os.path.basename(component_file).replace('.mha', '_smoothed.mha')}")
	
        # Here it is important to use itk.F if the input files are also type float. 
        PixelType = itk.F
        Dimension = 3
        ImageType = itk.Image[PixelType, Dimension]

        # Read the component file
        reader = itk.ImageFileReader[ImageType].New()
        reader.SetFileName(component_file)

        # Smooth the component
        smoothFilter = itk.SmoothingRecursiveGaussianImageFilter[ImageType, ImageType].New()
        smoothFilter.SetInput(reader.GetOutput())
        smoothFilter.SetSigma(sigma)  # Use the input sigma value

        # Write the smoothed component
        writer = itk.ImageFileWriter[ImageType].New()
        writer.SetFileName(output_file)
        writer.SetInput(smoothFilter.GetOutput())

        writer.Update()

        print(f"Component {component_file} smoothed and saved as {output_file}")

def generate_mesh(input_file, output_folder, contour_value, sigma):
    """This function generates a mesh from the input file. The input file is first divided into connected components, then smoothed, and finally, a mesh is generated for each component.

    Parameters
    ----------
    input_file : str
        The path to the input file that should be processed.
    output_folder : str
        The path to the output folder where the processed files should be saved.
    contour_value : float
        The contour value for generating the mesh.
    sigma : float
        The standard deviation of the Gaussian kernel used for smoothing.
    """
    
    # Change the type of the file to vtk. 
    component_files = extract_connected_components(input_file, output_folder)
    
    # Smooth the components
    smooth_components(component_files, output_folder, sigma)

    # This generates mesh for each smoothed component
    for component_file in component_files:
        smoothed_file = f"{os.path.splitext(component_file)[0]}_smoothed.mha"
        output_file = os.path.join(output_folder, f"{os.path.basename(smoothed_file).replace('.mha', '.vtk')}")

        #Read the ITK image data
        inputImage = itk.imread(smoothed_file)

        # Convert ITK image to VTK image data
        vtkImage = itk.vtk_image_from_image(inputImage)

        # Create a vtkContourFilter instance
        contourFilter = vtk.vtkContourFilter()
        contourFilter.SetInputData(vtkImage)
        contourFilter.SetValue(0, contour_value)  # Use the input contour value

        # Perform contour extraction
        contourFilter.Update()

        # Get the extracted mesh
        mesh = contourFilter.GetOutput()

        # Write the mesh to a file
        writer = vtk.vtkPolyDataWriter()
        writer.SetFileName(output_file)
        writer.SetInputData(mesh)
        writer.Write()

        print(f"Mesh saved for {smoothed_file} to {output_file}")


if __name__ == "__main__":
    # Create the argument parser to gain input from the terminal
    parser = argparse.ArgumentParser(description="Extract connected components, smooth them, and generate mesh.")
    parser.add_argument("input_file", help='Path to the input MHA file')
    parser.add_argument("output_folder", help="Output folder for saving the processed files")
    parser.add_argument("--sigma", type=float, default=1.0, help='This is the sigma value for the smoothing (the default value is (from Theory 3*voxelsize: 1.0)')
    parser.add_argument("--contour_value", type=float, default=0.5, help='This is the contour value for generating the mesh (the default value is: 0.5)')

    # Parses the command-line arguments
    args = parser.parse_args()

    input_file = args.input_file
    output_folder = args.output_folder
    sigma = args.sigma
    contour_value = args.contour_value
    
    # Check if the ITK version is at least 5.2.0
    required_version = packaging.version.parse("5.2.0")
    current_version = packaging.version.parse(itk.Version.GetITKVersion())
    
    if current_version < required_version:
        print(f"ITK {required_version} or newer is required.")
        sys.exit(1)

    generate_mesh(input_file, output_folder, contour_value, sigma)

