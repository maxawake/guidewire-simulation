#Here we import necessary libraries: os for just for interacting with the operating system, vtk is needed to use the vtk decimate pro filter, in order to be able to reduce the number of triangles available. This is implemented in order to analyze the influence of a lower resolution of the triangle mesh that represents the geometry of the mesh.
#We especially use the Unity project initially developed by Robin Viellieber in 2023. This Unity project is built and used for tests, it was adapted to allow easier and less error prone tests, that are fully automated. This includes automatic creation of guidewires, meshes and a more accurate simulation by simulating quasi-static and steady states only. The changes are explained in the bachelor thesis (ALexander Kreibich, 2023).
#Besides of this, I used the OBJ Importer by Dummiesman in the Unity Asset store and adapted it slightly, as well as using the vtk Decimate Pro filter (after this example: https://examples.vtk.org/site/Python/Meshes/Decimation/)
import sys
import os
import vtk
import gc
import subprocess
import time
import numpy as np

#First it is necassery to have a method that can converts a vtk file to an obj file. This is necassery as Unity only allows .obj files as an input fie for a triangular mesh structure.
def write_polydata_to_obj(polydata, obj_file):
#First the points are extracted from the file
    points = polydata.GetPoints()
    #This returns the number of points as an array
    num_points = points.GetNumberOfPoints()
    #Now we open the obj file that we want to convert to
    with open(obj_file, "w") as f:
    #This leads to a loop over all points (by the total number of points from above)
        for i in range(num_points):
        #Now we extract the position of the point
            point = points.GetPoint(i)
            #In the following we use the vertex information from above and put them into the obj file that we opened above. 
            f.write(f"v {point[0]} {point[1]} {point[2]}\n")
            #This accesses the cell array that defines the polygons. FOr the case of our meshes these are triangles, as we use the marching cubes algorithm to extract the isosurface.
        cell_array = polydata.GetPolys()
        #This initializes a traversal of the list of cells (of the triangles)
        cell_array.InitTraversal()
        #Now we creat an object to store the ids of points in a cell
        cell = vtk.vtkIdList()
        #This goes through each cell in the cell array
        while cell_array.GetNextCell(cell):
        #This takes the number of points in the current cell.
            num_cell_points = cell.GetNumberOfIds()
            #Now we want to check if this polygon we create is infact a triangle. We need to do this, as the decimate pro filter by vtk that we use later on wil only work, if we use only triangles, therefore we need to check if the cell is a triangle, so therefore has 3 points
            if num_cell_points == 3:
            #This writes the face information to the obj file.
                f.write(f"f {cell.GetId(0)+1} {cell.GetId(1)+1} {cell.GetId(2)+1}\n")
            else:
            #Or, as mentioned above, if it is not a triangle, we want to skip it and send an error message, this is quite critical to watch
                print(f"Skipping face with {num_cell_points} points")

#Defining a function for mesh processing, including reading an OBJ file, cleaning, decimating, and writing the output.
def process_mesh(input_obj, output_obj, reduction_value):
#First we check, if the input OBJ file exists
    if not os.path.exists(input_obj):
    #or else we print an error Message 
        print(f"Error: File {input_obj} not found.")
        return
        #Creating an OBJ reader to read the input OBJ file
    reader = vtk.vtkOBJReader()
    #Setting the file name for the reader
    reader.SetFileName(input_obj)
    #reading the file
    reader.Update()
    #Getting the output of the reader, which is a vtkPolyData object.
    initial_polydata = reader.GetOutput()
    #Creating a cleaner to remove duplicate points and cells, this is important in order to load a clipped (clipped in paraview) mesh and still decimate it
    cleaner = vtk.vtkCleanPolyData()
    #Setting the input for the cleaner as the output of the reader
    cleaner.SetInputConnection(reader.GetOutputPort())
    #Updating the cleaner to process the data
    cleaner.Update()
    #Create a decimator that we will then use for reducing the number of polygons in the mesh
    decimator = vtk.vtkDecimatePro()
    #Setting the input for the decimator as the output of the cleaner, so it has the wanted effect.
    decimator.SetInputConnection(cleaner.GetOutputPort())
    #Now we set the target reduction value. this is the value by which the number of triangles is to be reduced. e.g. if the reduction is 0.75, then 75% of the triangles are to be reduced. In contrdiction to the original decimation paper idea, this can change the topology and geometry of the mesh (slightly)
    decimator.SetTargetReduction(reduction_value)
    #again update the decimator, to process the data
    decimator.Update()
    #Taking the output of the decimator, which is the final, reduced vtkPolyData --> therefore the mesh we want with the reduction value we wanted
    final_polydata = decimator.GetOutput()
    #Now this data has to be saved to the specified output OBJ file
    write_polydata_to_obj(final_polydata, output_obj)

#Defining a function to run the built Unity application that simulates the guidewires movement
def run_unity_for_obj(obj_path, log_file_path, position, scale, rotation, time_step, rod_element_length, decimation_value, z_displacement, second_obj_path=None):
    #Here the path to the unity built needs to be entered to the correct path where it is saved. 
    unity_app_path = "/home/akreibich/TestRobinCode37/SimulatingGuidewiresInBloodVessels-main/GuidewireSimulation/GuidewireSimulation/TestFreeSpaceStop23.x86_64"
    #We simulate a command line environment, as the Unity built can access such arguments with a parser function. Therefore we create a list of command line arguments to pass to the Unity application (so the properties of the guidewire that is to be created, the mesh location etc.
    command_args = [
        unity_app_path,
        #"-batchmode",
        #"-nographics",
        "-objPath",
        obj_path,
        "-logFilePath",
        log_file_path,
        "-position",
        ",".join(map(str, position)),
        "-scale",
        ",".join(map(str, scale)),
        "-rotation",
        ",".join(map(str, rotation)),
        "-timeStep",
        str(time_step),
        "-rodElementLength",
        str(rod_element_length),
        "-decimationValue",
        str(decimation_value),
        "-zDisplacement",
        str(z_displacement)
    ]
    #Adding arguments for the second OBJ path if provided (this is onl necassery if not piercing from the outside is wanted either --> then a second mesh with inverted normales can be added, it has the precisly same properties as the first mesh automatically in the built Unity project
    if second_obj_path:
        command_args.extend(["-secondObjPath", second_obj_path])
    #Launching the Unity application with the specified arguments using subprocess
    proc = subprocess.Popen(command_args)
    #Returning the process object for further manipulation
    return proc

#Now the main execution block comes
if __name__ == "__main__":

    gc.disable()
    #Specify the path to the input OBJ file here, this is the mesh file (the processed mesh, that is undecimated) --> but it needs to be a triangular mesh structure of type .obj 
    input_obj_file = "/home/akreibich/Desktop/Bachelor/Data (Binary)/Input/Input_MeshOption/DataForMeshes/Data10/Component1_normals1809_pos.obj"
    #Specifying the number of reduction values to test
    n_values = 1
    #Generating a range of reduction values from a to b, evenly spaced (note: 0.1 = 10% reduction)
    reduction_values = np.linspace(0.4, 1, n_values)
    #Now we need a list to store the paths to the processed OBJ files, they will later be taken from this path and uploaded into the built Unity application. This is reasonable to do (save), because then the reduced meshes can also be checked manually, if everything worked in the decimation process
    obj_paths = []
    #Define the path to the file where debug velocities will be logged
    debug_velocities_file = "/home/akreibich/TestRobinCode37/DebugVelocities.txt"

    #Iterating over each reduction value --: take the mesh for each of them
    for k, reduction_value in enumerate(reduction_values):
        #Formatting the output file path with the reduction value, so where the files are stored
        output_obj_file = f"/home/akreibich/Desktop/GeneratedDecimatedMeshes/TestMeshesCore9/Component2_1111_pos.obj_test_{int(reduction_value * 100)}.obj"
        #Processing the mesh with the current reduction value and saving the output .obj file (path)
        process_mesh(input_obj_file, output_obj_file, reduction_value)
        #Adding the output OBJ path to the list, from this we will later take the mesh and upload it (using the OBJ Importer by Dummiesman from the Unity asset store)
        obj_paths.append(output_obj_file)
        
	#Defining the initial position, scale, and rotation for the Unity project, that gets passed to the mesh
        position = [1312, 84, -1162]
        scale = [32.2, 32.2, 32.2]
        rotation = [1.135, -86.79, -19.47]
	#Setting the initial time step for the simulation
        time_step = 0.005
	#Setting the initial length of the rod elements in the simulation
        rod_element_length = 10
	#Defining the number of iterations to run the simulation --> so many times are the parameters halved
        num_iterations = 4
	#Creating a list of displacement values in the z-direction to test in the simulation
        z_displacement_values = [0.5]

	#Iterating over each z displacement value --> so several times, each time till a steady state is reached. This prohibits an accumulation of errors
        for z_displacement in z_displacement_values:
		#Formatting the path for the log file for each iteration
            log_file_path = f"/home/akreibich/TestRobinCode37/Position#N.txt"
#		Creating a string to log the start of a new iteration with current parameters
            iteration_info = f"New Iteration {k}, decimation: {reduction_value * 100}%, rod_element_length: {rod_element_length}, time_step: {time_step}, z_displacement: {z_displacement}\n"
            
#		Opening the log file and appending the iteration start information, to save all the information for each single simulation and have them bundeled together for the analysis later
            with open(log_file_path, 'a') as f:
                f.write(iteration_info)
            
	    #Open the debug velocities file and appending the iteration start information, to save these (-""-)
            with open(debug_velocities_file, 'a') as f:
                f.write(iteration_info)

	    #Running the simulation for a number of iterations defined earlier
            for i in range(num_iterations):
		 #Recording the start time of the iteration
                start_time = time.time()
		 #Here we run the Unity built for the currently specified .obj file with the specified parameters
                proc = run_unity_for_obj(obj_paths[-1], log_file_path, position, scale, rotation, time_step, rod_element_length, reduction_value, z_displacement)
                
                #Entering a loop to wait for the Unity process to complete. Here we check in short time intervals, if the application has already been finished = if the final steady state has been reached. THis happens, if the exit codes = 0, as this is the code that the simulation has sucessfully finished = reached its final state. We also measure the time between each steady state and record it.
                while True:
                    
                    if proc.poll() is not None:
                       
                        exit_code = proc.returncode
                      
                        if exit_code == 1:
                            end_time = time.time()
                            total_time = end_time - start_time
                            with open(log_file_path, 'a') as f:
                                f.write(f"Total time for this run: {total_time} seconds\n")
                            break

                        elif exit_code == 0:
                            break
                   
                    time.sleep(0.1)

		#Halving the rod_element_length and time_step for the next iteration to increase simulation accuracy --> this influence is analyzed, therefore we repeat the same measurement several times for the same mesh and other parameters. Instead of changing the rod_element_length we can also change the time_step in the same way/ idea
                rod_element_length /= 2
                time_step /= 1

		#Log string/ information output for the updated parameters for the next iteration
                iteration_info = f"Updated for next iteration: rod_element_length: {rod_element_length}, time_step: {time_step}\n"
		#Always writing the updated parameters to the log file
                with open(log_file_path, 'a') as f:
                    f.write(iteration_info)
		#Writing the updated parameters to the debug velocities file, with this we never have to guess what parameters were used, but store all information in the relevant files
                with open(debug_velocities_file, 'a') as f:
                    f.write(iteration_info)

    
    gc.enable()
