import subprocess

commands = [
    ["/home/akreibich/TestRobinCode3/SimulatingGuidewiresInBloodVessels-main/GuidewireSimulation/GuidewireSimulation/Test3Build3.x86_64", "0.5"],
    ["/home/akreibich/TestRobinCode3/SimulatingGuidewiresInBloodVessels-main/GuidewireSimulation/GuidewireSimulation/Test3Build3.x86_64", "10"]
]

for cmd in commands:
    process = subprocess.Popen(cmd, stdout=subprocess.PIPE)
    try:
        process.wait(timeout=30)
        print(process.stdout.read())
    except subprocess.TimeoutExpired:
        print(f"Command '{' '.join(cmd)}' timed out after 30 seconds.")
        process.kill()
