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


def run_unity(parameters: str, log_file_path: str):
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
    unity_app_path = "/home/max/Documents/Unity/guidewire-simulation-static.x86_64"

    command_args = [unity_app_path, "-parameters", parameters, "-logFile", log_file_path]

    proc = subprocess.run(command_args)
    return proc
