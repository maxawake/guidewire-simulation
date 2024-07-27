import json
import os
import subprocess


def read_json_file(file_path):
    """Function to read a JSON file.

    Parameters
    ----------
    file_path : str
        Path to the JSON file.

    Returns
    -------
    dict
        Dictionary containing the JSON data.
    """

    with open(file_path, "r") as f:
        data = json.load(f)
    return data


def save_json_file(file_path, data):
    """Function to save a dictionary to a JSON file.

    Parameters
    ----------
    file_path : str
        Path to the JSON file.
    data : dict
        Dictionary to be saved.
    """

    with open(file_path, "w") as f:
        json.dump(data, f)


def run_unity(unity_app_path: str, parameters: str, log_file_path: str, headless: bool = False, verbose: bool = False):
    """Function to run the Unity application for a given OBJ file and parameters.

    Parameters
    ----------
    obj_path : str
        Path to the OBJ file.
    log_file_path : str
        Path to the log file.
    decimation_value : float
        The decimation value.
    z_displacement : float
        The z displacement.
    params : dict
        Dictionary containing the position, scale, rotation, time step, and rod element length.
    second_obj_path : str, optional
        Path to the second OBJ file, by default None

    Returns
    -------
    subprocess.Popen
        Unity process.
    """

    command_args = [unity_app_path, "-parameters", parameters, "-logFile", log_file_path, "-monitor", "2"]
    
    if headless:
        command_args.append("-batchmode")
        command_args.append("-nographics")
        
        
    if not verbose:
        stdout = subprocess.DEVNULL
    else:
        stdout = None

    print("Running Unity with the following command:")
    print(" ".join(command_args))
    proc = subprocess.run(command_args, stdout=stdout)
    print("Unity process finished with exit code:", proc.returncode)
    return proc


def set_nice(niceness):
    """Function to set the niceness of the process.

    Parameters
    ----------
    niceness : int
        Niceness value. 
    """
    val = os.nice(niceness)
    print("Set niceness to: ", val, "\n")
    

def run_expriment(unity_path, path, name, parameters):
    """Function to run an expriment.

    Parameters
    ----------
    unity_path : str
        Path to the Unity application.
    path : str
        Path to the folder where the experiment will be saved.
    name : str
        Name of the experiment.
    parameters : dict
        Dictionary containing the parameters of the experiment.
    """
    current_folder = os.path.join(path, name)
    parameter_path = os.path.join(current_folder, "parameters.json")
    log_path = os.path.join(current_folder, "log.txt")
    
    # create a new folder for each experiment
    os.makedirs(current_folder, exist_ok=True)
    
    # save the parameters to a JSON file
    parameters["logFilePath"] = current_folder + "/"
    save_json_file(parameter_path, parameters)
    
    # run the Unity application
    run_unity(unity_path, parameter_path, log_path)

def prepare_experiment(name, path):
    """Function to prepare an experiment.

    Parameters
    ----------
    name : str
        Name of the experiment.
    path : str
        Path to the folder where the experiment will be saved.
    
    Returns
    -------
    str
        Path to the experiment folder.
    str
        Name of the experiment.
    """
    experiment_folder = os.path.join(path, name)
    os.makedirs(experiment_folder, exist_ok=True)
    return experiment_folder, name