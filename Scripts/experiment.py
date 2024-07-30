from utils import *
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit
import numpy as np
from scipy import signal

plt.style.use("light")
import cmasher as cmr


class GuidewireExperiment:
    def __init__(self, positions):
        self.positions = positions
        self.timesteps = len(self.positions)
        self.n_spheres = len(self.positions["0"]["sphere"])
        self.sphere_pos = self.get_all_spheres()
        self.total_time = self.get_total_time()
        self.elapsed_time = self.get_elapsed_time()
        self.delta = self.get_delta()

    def get_all_spheres(self):
        return np.array([self.get_sphere_position(idx) for idx in range(self.n_spheres)])

    def get_sphere_position(self, idx):
        sphere_pos = np.array(
            [
                np.array(
                    [
                        self.positions[str(i)]["sphere"][str(idx)]["x"],
                        self.positions[str(i)]["sphere"][str(idx)]["y"],
                        self.positions[str(i)]["sphere"][str(idx)]["z"],
                    ]
                )
                for i in range(0, self.timesteps)
            ]
        )
        return sphere_pos  # - sphere_pos[0]

    def get_total_time(self):
        total_time = np.array([self.positions[str(i)]["totalTime"] for i in range(0, self.timesteps)])
        return total_time

    def get_elapsed_time(self):
        elapsed_time = [self.positions["0"]["elapsedMilliseconds"]]
        for i in range(1, self.timesteps):
            elapsed_time.append(elapsed_time[-1] + self.positions[str(i)]["elapsedMilliseconds"])
        # elapsed_time = [self.positions[str(i)]["elapsedMilliseconds"] for i in range(0, self.timesteps)]
        return np.array(elapsed_time)

    def get_delta(self):
        delta = np.array([self.positions[str(i)]["delta"] for i in range(0, self.timesteps)])
        return delta

    def plot_sphere(self, idx, axis=0):
        sphere_pos = self.get_sphere_position(idx)

        plt.figure(figsize=(5, 5), dpi=70)
        plt.plot(sphere_pos[:, axis])
        plt.xlabel("Timestep")
        plt.ylabel("Position [units]")
        plt.show()

    def plot_all_spheres(self, axis=0):
        sphere_matrix = self.sphere_pos[:, :, axis] - self.sphere_pos[:, 0, axis, np.newaxis]
        # sphere_matrix -= np.min(sphere_matrix)

        plt.figure(figsize=(5, 4))
        plt.imshow(sphere_matrix, aspect="auto", cmap="cmr.viola", origin="lower", extent=[0, self.get_total_time()[-1], 0, self.n_spheres], vmin=0, vmax=20)
        plt.xlabel("In-Game Time [s]")
        plt.ylabel("Sphere Index")
        plt.colorbar(label="Displacement [units]")
        # plt.show()

    def plot_all_spheres2(self, axis=0):
        sphere_matrix = self.sphere_pos[:, :, axis] - self.sphere_pos[:, 0, axis, np.newaxis]
        # sphere_matrix -= np.min(sphere_matrix)

        plt.figure(figsize=(6, 5), dpi=70)
        plt.imshow(
            sphere_matrix[:, int(sphere_matrix.shape[1] * 0.8) :],
            aspect="auto",
            cmap="cmr.redshift",
            extent=[self.get_total_time()[-1] * 0.8, self.get_total_time()[-1], self.n_spheres, 0],
        )
        plt.xlabel("In-Game Time [s]")
        plt.ylabel("Sphere Index")
        plt.colorbar(label="Displacement [units]")
        # plt.show()

    def plot_experiment(self, axis=2):

        time_array = self.get_total_time()
        sphere_pos = self.get_sphere_position(self.n_spheres - 1)

        fig, ax = plt.subplots(3, 1, figsize=(5, 6))

        ax[0].plot(time_array, self.get_elapsed_time() / 1000)
        ax[0].set_title("(a) Total Time")
        ax[0].set_ylabel("Real time $T$ [s]")
        ax[0].set_xlabel("In-Game Time $t$ [s]")

        ax[1].plot(time_array, sphere_pos[:, axis])
        ax[1].set_title("(b) Last Sphere Absolute Position")
        ax[1].set_ylabel("Position $z(t)$ [units]")
        ax[1].set_xlabel("In-Game Time $t$ [s]")

        ax[2].plot(time_array, self.get_delta())
        ax[2].set_yscale("log")
        ax[2].set_title("(c) Internal prediction Error")
        ax[2].set_ylabel("Error $\Delta_p$ [units]")
        ax[2].set_xlabel("In-Game Time $t$ [s]")

        plt.tight_layout()
        # plt.show()




def func(x, a, b):
    return a * np.exp(-x / b)  # +c


def get_decay_rate(experiment, p0, offset, debug=False, save=False):
    xs = experiment.get_total_time()
    # data = experiment.get_sphere_position(experiment.n_spheres-1)[:,2]

    sph = experiment.get_all_spheres()
    sph = sph - sph[:, 0, np.newaxis]
    data = sph[-1, :, 2]  # - data[0]
    
    data = np.abs(data - offset)

    # Find the upper peaks of the damped oscillation
    peak_idxs, _ = signal.find_peaks(data)

    peaks_positions = xs[peak_idxs]
    peaks_values = data[peak_idxs]
    
    error = sph[:, peak_idxs[-1], 2] - offset

    p0 = [peaks_values[0], xs[peak_idxs[1]] - xs[peak_idxs[0]]]

    # Fit exponential decay to the peaks
    popt, pcov = curve_fit(func, peaks_positions, peaks_values, sigma=0.001, p0=p0)

    if debug:
        plt.figure(figsize=(4, 3))
        plt.title(f"Relaxation time: {popt[1]:.2f} $s^{-1}$")
        plt.plot(xs, data, label="Data", color="black")
        plt.plot(xs, func(xs, *popt), label="Fitted Curve")
        plt.plot(xs[peak_idxs], data[peak_idxs], ".", color="red", label="Peaks")
        # plt.hlines(p0[-1], xs[0], xs[-1], linestyle="--", label="Expected offset", color="darkgray")
        plt.xlabel("In-Game Time [s]")
        plt.ylabel("Displacement Error $\Delta_{\mathbf{x},n+1}$ [units]")
        #plt.yscale("log")
        plt.legend(prop={"size": 10})
        if save:
            plt.savefig("/home/max/Nextcloud/Praktikum/Report/figures/longitudinal/experiment1_3.pdf", dpi=300, bbox_inches="tight")
        plt.show()

        plt.figure(figsize=(4, 3))
        plt.title(f"Final Error $\Delta_{{\mathbf{{x}}}}$: {np.mean(np.abs(error)):.4f} units")
        plt.plot(error, "-o")
        plt.xlabel("Sphere Index $i$")
        plt.ylabel("Displacement Error $\Delta_{\mathbf{x},i}$ [units]")
        if save:
            plt.savefig("/home/max/Nextcloud/Praktikum/Report/figures/longitudinal/experiment1_4.pdf", dpi=300, bbox_inches="tight")
        plt.show()

    return popt, peak_idxs#error


def get_all_data(name, parameters, repeat, path, debug=False):
    relaxation_times = []
    offsets = []
    loop_times = []
    errors = []
    params_ = []

    for run in range(repeat):
        relaxation_time = []
        offset = []
        loop_time = []
        error_ = []
        param = []

        for p in parameters:
            positions = read_json_file(path + f"{name}_{run}/{name}_{run}_{p}/positions.json", verbose=False)
            params = read_json_file(path + f"{name}_{run}/{name}_{run}_{p}/parameters.json", verbose=False)
            #print(path + f"{name}_{run}/{name}_{run}_{p}")
            experiment = GuidewireExperiment(positions)
            #print("Timesteps: ", experiment.timesteps)
            popt, peak_idxs = get_decay_rate(experiment, debug=debug, p0=[1.2, 0.01], offset=params["displacement"])  # , params["displacement"]])
            decay_rate, omega = popt[1], popt[0]
            times = experiment.get_total_time()[-1]  # np.diff(experiment.get_total_time())
            times = np.diff(experiment.get_elapsed_time())

            loop_time.append(np.mean(times))
            relaxation_time.append(decay_rate)
            offset.append(omega)
            if experiment.get_elapsed_time().shape[0] > 4000:
                print("Not converged")

            error_.append([experiment.get_elapsed_time()[peak_idxs[-1]]/1000, experiment.get_total_time()[peak_idxs[-1]]])#np.mean(np.abs(error)))
            param.append(params)

        relaxation_times.append(relaxation_time)
        offsets.append(offset)
        loop_times.append(loop_time)
        errors.append(error_)
        params_.append(param)
    return np.array(relaxation_times), np.array(offsets), np.array(loop_times), np.array(errors), params_


def plot_data(x, y, xlabel, ylabel):
    fig = plt.figure(figsize=(4, 3))
    plt.plot(x, y, "o-")
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    # return fig

def set_labels(ax, xlabel):
    ax[0,0].set_title("(a)")
    ax[0,1].set_title("(b)")
    ax[1,0].set_title("(c)")
    ax[1,1].set_title("(d)")

    ax[0,1].set_xlabel(xlabel)
    ax[1,1].set_xlabel(xlabel)
    ax[1,0].set_xlabel(xlabel)

    ax[0,1].set_ylabel("Average Step Time [ms]")
    #ax[2].set_ylabel("Total Error $\Delta_{{\mathbf{{x}}}}$ [units]")
    ax[1,1].set_ylabel("Computation Time [s]")
    ax[1,0].set_ylabel("In-Game Time [s]")
    
def plot_confidence_interval(ax, x, y):
    # fig = plt.figure(figsize=(12, 5), dpi=300)

    # Get percentiles for the confidence interval
    p10 = np.percentile(y, axis=0, q=[10]).ravel()[:]
    p90 = np.percentile(y, axis=0, q=[90]).ravel()[:]
    p25 = np.percentile(y, axis=0, q=[25]).ravel()[:]
    p75 = np.percentile(y, axis=0, q=[75]).ravel()[:]

    # Plot mean and confidence
    ax.fill_between(x, p10, p90, alpha=0.1, color="b", label="$90\%$ CI")
    ax.fill_between(x, p25, p75, alpha=0.25, color="b", label="$75\%$ CI")
    ax.plot(x, np.mean(y, axis=0)[:], "o-", color="black", label="Mean")

    # Labels
    
    ax.legend(prop={"size": 10})
    # return fig


COLORS = ["#9e0142", "#d53e4f", "#f46d43", "#fdae61", "#fee08b", "#ffffbf", "#e6f598", "#abdda4", "#66c2a5", "#3288bd", "#5e4fa2"]

def plot_transversal( PATH, SAVE_PATH, name, params, reference, xlabel, save=False, format="int", log=False):
    total_error = []

    fig, ax = plt.subplots(1,3,figsize=(10,3))# plt.figure(figsize=(4,3))

    for i,param in enumerate(params):
        try:
            positions = read_json_file(PATH + f"{name}_0/{name}_0_{param}/positions.json", verbose=False)

            experiment = GuidewireExperiment(positions)

            # Get distance to reference
            pos_ref = reference.get_all_spheres()[::(reference.n_spheres-1)//(experiment.n_spheres-1)]
            pos = experiment.get_all_spheres()
            
            initial_positions = pos[:,0,2].copy()
            final_pos = pos[:,-1,:].copy()
            
            dist = np.linalg.norm(pos_ref - pos, axis=2)
            
            # Save average distance
            total_error.append(np.mean(dist[:,-1]))

            if format == "int":
                ax[1].plot(initial_positions, dist[:,-1], ".-", label=f"{int(param)}", color=COLORS[i])
                ax[0].plot(final_pos[:,2], final_pos[:,1], ".-", label=f"{int(param)}", color=COLORS[i])
            if format == "sci":
                ax[1].plot(initial_positions, dist[:,-1], ".-", label=f"{param:.2e}", color=COLORS[i])
                ax[0].plot(final_pos[:,2], final_pos[:,1], ".-", label=f"{param:.2e}", color=COLORS[i])
            if format == "float":
                ax[1].plot(initial_positions, dist[:,-1], ".-", label=f"{param:.2f}", color=COLORS[i])
                ax[0].plot(final_pos[:,2], final_pos[:,1], ".-", label=f"{param:.2f}", color=COLORS[i])
                
            

            ax[1].set_title("(b) " + xlabel)
            ax[1].set_ylabel("Distance [units]")
            ax[1].set_xlabel("Position $z$ [units]")
        except Exception as e:
            print(f"Error in {param}")
            print(e)
        
    ax[1].legend(prop={"size": 6})
    
    ax[2].plot(params, total_error, "o-", color="black")
    ax[2].set_title("(c) Average Distance to Reference")
    ax[2].set_ylabel("Distance [units]")
    ax[2].set_xlabel(xlabel)

    if log:
        ax[2].set_xscale("log")

    final_pos = pos_ref[:,-1,:]
    ax[0].plot(final_pos[:,2], final_pos[:,1], ".-", color="black", label="Reference")
    ax[0].legend(prop={"size": 6})
    ax[0].set_title("(a) Final Sphere Position")
    ax[0].set_xlabel("Position $z$ [units]")
    ax[0].set_ylabel("Position $y$ [units]")
        
    plt.tight_layout()
    
    if save:
        plt.savefig(SAVE_PATH + f"{name}.pdf", bbox_inches="tight", dpi=300)
    plt.show()
    
    plt.figure(figsize=(4,3))
    print(dist.max(),dist.min())
    plt.imshow(dist, aspect="auto", cmap=cmr.lavender, extent=[0, experiment.get_total_time()[-1], experiment.n_spheres, 0], interpolation="bicubic")
    #plt.title("Time-Evolution of Distance to Reference")
    plt.xlabel("In-Game Time [s]")
    plt.ylabel("Sphere Index $i$")
    plt.colorbar(label="Distance [units]")
    if save:
        plt.savefig(SAVE_PATH + f"{name}_heatmap.pdf", bbox_inches="tight", dpi=300)
    plt.show()