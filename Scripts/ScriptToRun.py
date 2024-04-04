# Unity project initially developed by Robin Viellieber in 2023. This Unity project is built and used for tests, it was adapted to allow easier and less error prone tests, that are fully automated. This includes automatic creation of guidewires, meshes and a more accurate simulation by simulating quasi-static and steady states only. The changes are explained in the bachelor thesis (ALexander Kreibich, 2023).
# Besides of this, I used the OBJ Importer by Dummiesman in the Unity Asset store and adapted it slightly, as well as using the vtk Decimate Pro filter (after this example: https://examples.vtk.org/site/Python/Meshes/Decimation/)
import sys
import os
import vtk
import gc
import subprocess
import time
import numpy as np


def write_polydata_to_obj(polydata, obj_file):
    """Converts a vtk file to an obj file. This is necassery as Unity only allows .obj files as an input fie for a triangular mesh structure.

    Parameters
    ----------
    polydata : _type_
        _description_
    obj_file : _type_
        _description_
    """

    points = polydata.GetPoints()
    num_points = points.GetNumberOfPoints()

    # Now we open the obj file that we want to convert to
    with open(obj_file, "w") as f:
        # This leads to a loop over all points (by the total number of points from above)
        for i in range(num_points):
            point = points.GetPoint(i)
            f.write(f"v {point[0]} {point[1]} {point[2]}\n")

        # This initializes a traversal of the list of cells (of the triangles)
        cell_array = polydata.GetPolys()
        cell_array.InitTraversal()

        # Now we creat an object to store the ids of points in a cell
        cell = vtk.vtkIdList()

        while cell_array.GetNextCell(cell):
            num_cell_points = cell.GetNumberOfIds()
            # Now we want to check if this polygon we create is infact a triangle. We need to do this, as the decimate pro filter by vtk that we use later on wil only work, if we use only triangles, therefore we need to check if the cell is a triangle, so therefore has 3 points
            if num_cell_points == 3:
                f.write(f"f {cell.GetId(0)+1} {cell.GetId(1)+1} {cell.GetId(2)+1}\n")
            else:
                print(f"Skipping face with {num_cell_points} points")


def process_mesh(input_obj, output_obj, reduction_value):
    """Function for mesh processing, including reading an OBJ file, cleaning, decimating, and writing the output.

    Parameters
    ----------
    input_obj : _type_
        _description_
    output_obj : _type_
        _description_
    reduction_value : _type_
        _description_

    Returns
    -------
    _type_
        _description_
    """
    # First we check, if the input OBJ file exists
    if not os.path.exists(input_obj):
        print(f"Error: File {input_obj} not found.")
        return

    # Creating a reader to read the input OBJ file
    reader = vtk.vtkOBJReader()
    reader.SetFileName(input_obj)
    reader.Update()

    # Creating a cleaner to remove duplicate points and cells, this is important in order to load a clipped (clipped in paraview) mesh and still decimate it
    cleaner = vtk.vtkCleanPolyData()
    cleaner.SetInputConnection(reader.GetOutputPort())
    cleaner.Update()

    # Create a decimator that we will then use for reducing the number of polygons in the mesh
    decimator = vtk.vtkDecimatePro()
    decimator.SetInputConnection(cleaner.GetOutputPort())
    # Now we set the target reduction value. this is the value by which the number of triangles is to be reduced. e.g. if the reduction is 0.75, then 75% of the triangles are to be reduced. In contrdiction to the original decimation paper idea, this can change the topology and geometry of the mesh (slightly)
    decimator.SetTargetReduction(reduction_value)
    decimator.Update()

    # Writing the output of the decimator to the output OBJ file
    write_polydata_to_obj(decimator.GetOutput(), output_obj)


def run_unity_for_obj(
    obj_path, log_file_path, decimation_value, z_displacement, params, second_obj_path=None
):
    """Defining a function to run the built Unity application that simulates the guidewires movement.

    Parameters
    ----------
    obj_path : _type_
        _description_
    log_file_path : _type_
        _description_
    position : _type_
        _description_
    scale : _type_
        _description_
    rotation : _type_
        _description_
    time_step : _type_
        _description_
    rod_element_length : _type_
        _description_
    decimation_value : _type_
        _description_
    z_displacement : _type_
        _description_
    second_obj_path : _type_, optional
        _description_, by default None

    Returns
    -------
    _type_
        _description_
    """

    unity_app_path = "/home/max/Nextcloud/Praktikum/Code/guidewire-simulation/Unity/test_max.x86_64"

    command_args = [
        unity_app_path,
        "-batchmode",
        "-nographics",
        "-objPath",
        obj_path,
        "-logFilePath",
        log_file_path,
        "-position",
        ",".join(map(str, params["position"])),
        "-scale",
        ",".join(map(str, params["scale"])),
        "-rotation",
        ",".join(map(str, params["rotation"])),
        "-timeStep",
        str(params["time_step"]),
        "-rodElementLength",
        str(params["rod_element_length"]),
        "-decimationValue",
        str(decimation_value),
        "-zDisplacement",
        str(z_displacement),
    ]

    # Adding arguments for the second OBJ path if provided (this is onl necassery if not piercing from the outside is wanted either --> then a second mesh with inverted normales can be added, it has the precisly same properties as the first mesh automatically in the built Unity project
    if second_obj_path:
        command_args.extend(["-secondObjPath", second_obj_path])
        
    # Launching the Unity application with the specified arguments using subprocess
    proc = subprocess.Popen(command_args)
    return proc


def check_termination(log_file_path, start_time, proc):
    """Function to check if the Unity process has terminated and measure the total time for the simulation.
    Here we check in short time intervals, if the application has already been finished = if the final steady state has been reached. 
    This happens, if the exit codes = 0, as this is the code that the simulation has sucessfully finished = reached its final state. 
    We also measure the time between each steady state and record it.

    Parameters
    ----------
    log_file_path : str
        Path to the log file.
    start_time : float
        Start time of the simulation.
    proc : subprocess.Popen
        Unity process.
    """
    while True:
        if proc.poll() is not None:
            exit_code = proc.returncode
            if exit_code == 1:
                end_time = time.time()
                total_time = end_time - start_time
                with open(log_file_path, "a") as f:
                    f.write(f"Total time for this run: {total_time} seconds\n")
                break
            elif exit_code == 0:
                break
        time.sleep(0.1)


def write_logs(log_file_path, debug_velocities_file, iteration_info):
    """Function to write the iteration information to the log files.

    Parameters
    ----------
    log_file_path : str
        Path to the log file.
    debug_velocities_file : str
        Path to the debug velocities file.
    iteration_info : str
        Information about the current iteration.
    """
    # Opening the log file and appending the iteration start information, to save all the information for each single simulation and have them bundeled together for the analysis later
    with open(log_file_path, "a") as f:
        f.write(iteration_info)

    # Open the debug velocities file and appending the iteration start information, to save these (-""-)
    with open(debug_velocities_file, "a") as f:
        f.write(iteration_info)


if __name__ == "__main__":
    # Disabling the garbage collector to avoid memory issues
    gc.disable()
    # Specify the path to the input OBJ file here, this is the mesh file (the processed mesh, that is undecimated) --> but it needs to be a triangular mesh structure of type .obj
    input_obj_file = "/run/media/max/Data/Simulations/Guidewire/Meshes/Input_MeshOption/DataForMeshes/Data10/Component1_normals1809_pos.obj"
    # Creating an empty list to store the paths to the output OBJ files
    obj_paths = []
    # Define the path to the file where debug velocities will be logged
    debug_velocities_file = "/home/max/Temp/Praktikum/DebugVelocities.txt"
    
    # Defining the initial position, scale, and rotation for the Unity project, that gets passed to the mesh
    position = [1312, 84, -1162]
    scale = [35.2, 35.2, 35.2]
    rotation = [1.135, -86.79, -19.47]
    time_step = 0.005
    rod_element_length = 10
    params = {"position": position, "scale": scale, "rotation": rotation, "time_step": time_step, "rod_element_length": rod_element_length}

    # Initial values for the simulation parameters
    num_iterations = 4
    z_displacement_values = [0.5]
    
    # Specifying the number of reduction values to test
    n_values = 1
    # Generating a range of reduction values from a to b, evenly spaced (note: 0.1 = 10% reduction)
    reduction_values = np.linspace(0.975, 1, n_values)

    # Iterating over each reduction value --: take the mesh for each of them
    for k, reduction_value in enumerate(reduction_values):
        # Formatting the output file path with the reduction value, so where the files are stored
        output_obj_file = (
            f"/run/media/max/Data/Simulations/Guidewire/Meshes/GeneratedDecimatedMeshes/Component2_1111_pos.obj_test_{int(reduction_value * 100)}.obj"
        )
        # Processing the mesh with the current reduction value and saving the output .obj file (path)
        process_mesh(input_obj_file, output_obj_file, reduction_value)
        obj_paths.append(output_obj_file)

        # Iterating over each z displacement value --> so several times, each time till a steady state is reached. This prohibits an accumulation of errors
        for z_displacement in z_displacement_values:
            # Formatting the path for the log file for each iteration
            log_file_path = f"/home/max/Temp/Praktikum/Position#N.txt"

            # Creating a string to log the start of a new iteration with current parameters
            iteration_info = f"New Iteration {k}, decimation: {reduction_value * 100}%, rod_element_length: {rod_element_length}, time_step: {time_step}, z_displacement: {z_displacement}\n"

            # Writing the iteration information to the log files
            write_logs(log_file_path, debug_velocities_file, iteration_info)

            # Running the simulation for a number of iterations defined earlier
            for i in range(num_iterations):
                # Recording the start time of the iteration
                start_time = time.time()
                
                # Here we run the Unity built for the currently specified .obj file with the specified parameters
                proc = run_unity_for_obj(obj_paths[-1], log_file_path, reduction_value, z_displacement, params)

                # Entering a loop to wait for the Unity process to complete. 
                check_termination()

                # Halving the rod_element_length and time_step for the next iteration to increase simulation accuracy --> this influence is analyzed, therefore we repeat the same measurement several times for the same mesh and other parameters. Instead of changing the rod_element_length we can also change the time_step in the same way/ idea
                rod_element_length /= 2
                time_step /= 1

                # Log string/ information output for the updated parameters for the next iteration
                iteration_info = f"Updated for next iteration: rod_element_length: {rod_element_length}, time_step: {time_step}\n"

                # Always writing the updated parameters to the log file
                write_logs(log_file_path, debug_velocities_file, iteration_info)

    # Re-enabling the garbage collector
    gc.enable()
