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
        plt.imshow(sphere_matrix, aspect="auto", cmap="cmr.redshift", extent=[0, self.get_total_time()[-1], self.n_spheres, 0])
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
        ax[2].set_title("(c) Internal prediction Error")
        ax[2].set_ylabel("Error $\Delta_p$ [units]")
        ax[2].set_xlabel("In-Game Time $t$ [s]")

        plt.tight_layout()
        # plt.show()


def plot_confidence_interval(ax, x, y, xlabel, ylabel):
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
    ax.set_xlabel(xlabel)
    ax.set_ylabel(ylabel)
    ax.legend(prop={"size": 10})
    # return fig


def func(x, a, b):
    return a * np.exp(-x / b)  # +c


def get_decay_rate(experiment, p0, offset, debug=False, save=False):
    xs = experiment.get_total_time()
    # data = experiment.get_sphere_position(experiment.n_spheres-1)[:,2]

    sph = experiment.get_all_spheres()
    sph = sph - sph[:, 0, np.newaxis]
    data = sph[-1, :, 2]  # - data[0]
    error = sph[:, -1, 2] - offset
    data = np.abs(data - offset)

    # Find the upper peaks of the damped oscillation
    peak_idxs, _ = signal.find_peaks(data)

    peaks_positions = xs[peak_idxs]
    peaks_values = data[peak_idxs]

    p0 = [peaks_values[0], xs[peak_idxs[1]] - xs[peak_idxs[0]]]

    # Fit exponential decay to the peaks
    popt, pcov = curve_fit(func, peaks_positions, peaks_values, sigma=0.001, p0=p0)

    if debug:
        plt.figure(figsize=(4, 3))
        plt.title(f"Relaxation time: {popt[1]:.2f} $s^{-1}$")
        plt.plot(xs, data, label="Data", color="black")
        plt.plot(xs, func(xs, *popt), label="Fitted Curve")
        plt.plot(xs[peak_idxs], data[peak_idxs], "o", color="red", label="Peaks")
        # plt.hlines(p0[-1], xs[0], xs[-1], linestyle="--", label="Expected offset", color="darkgray")
        plt.xlabel("In-Game Time [s]")
        plt.ylabel("Displacement Error $\Delta_{z,n+1}$ [units]")
        plt.legend()
        if save:
            plt.savefig("/home/max/Nextcloud/Praktikum/Report/figures/experiment1_3.pdf", dpi=300, bbox_inches="tight")
        plt.show()

        plt.figure(figsize=(4, 3))
        plt.title(f"Final Error $\Delta_z$: {np.sum(error):.4f} units")
        plt.plot(error, "-o")
        plt.xlabel("Sphere Index $i$")
        plt.ylabel("Displacement Error $\Delta_{z_i}$ [units]")
        if save:
            plt.savefig("/home/max/Nextcloud/Praktikum/Report/figures/experiment1_4.pdf", dpi=300, bbox_inches="tight")
        plt.show()

    return popt, error


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
            positions = read_json_file(path + f"{name}_{run}/{name}_{run}_{p}/positions.json")
            params = read_json_file(path + f"{name}_{run}/{name}_{run}_{p}/parameters.json")
            experiment = GuidewireExperiment(positions)

            popt, error = get_decay_rate(experiment, debug=debug, p0=[1.2, 0.01], offset=params["displacement"])  # , params["displacement"]])
            decay_rate, omega = popt[1], popt[0]
            times = experiment.get_total_time()[-1]  # np.diff(experiment.get_total_time())
            times = np.diff(experiment.get_elapsed_time())

            loop_time.append(np.mean(times))
            relaxation_time.append(decay_rate)
            offset.append(omega)
            error_.append(np.sum(np.abs(error)))
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
