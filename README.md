# Simulating Guidewires in Blood Vessels using Cosserat Rod Theory

This project was created as part of my master thesis.
The objective of this thesis was to model a guidewire using the Cosserat Rod theory and simulate the interaction between a guidewire and blood vessels.

## Installation

This project can either be opened in Unity, which allows for a full access and customization of the implementation.
For this, the Unity Editor with version 2020.3.29f1 (LTS) or higher must be downloaded and installed [from here](https://unity3d.com/get-unity/download).
Once Unity is installed, clone the project to your local machine and open it with Unity Hub (Open --> Add project from disk).

Alternatively, several API's are available as well for an easy integration into other existing Unity projects in the form of packages. These are available in the Packages folder. Simply import these into your project with Assets -> Import Package -> Custom Package.
Assets like guidewires of different lengths can also be imported as assets with Assets -> Import New Asset.

## Getting Started

The two central pieces are the *Simulation* and the *Guidewire* GameObjects.

The *Guidewire* GameObject holds all *Spheres* and *Cylinder* GameObjects that the guidewire consists of. If *Spheres* or *Cylinders* are added or removed, the *Simulation Loop* component of the *Simulation* GameObject has to be modified respondingly.

The *Simulation* GameObject holds all components that run the algorithm. Several parameters of these components can be modified within the Unity Editor to change the way the simulation is running.
Examples are to select the constraints that should be active, or wether to run the constraint solving step in bilateral interleaving order or in naive order.

### Importing into an existing Project

For an importation into an existing project, several settings have to be set.
- The assembly definition containing the imported code must have th "Allow Unsafe Code" checked to be able to use BulletSharp.
- The collision of kinematic-kinematic pairs has to be enabled. To do this, go to Project Settings -> Physics -> Contact Pairs Mode -> Enable Kinematic Kinematic Pairs.
- Collision is only necessary between the guidewire and the blood vessel, i.e. no self collision. To turn the self collision of the guidewire off, go to Project Settings -> Physics -> Layer Collision Matrix -> Uncheck Guidewire Guidewire Box.
- If you modify an existing guidewire prefab, each sphere and cylinder primitive must have the layer "Guidewire".
- If you add other blood vessel objects, i.e. objects the guidewire should collide with, the object has to have the layer "Blood Vessel".


### Executing Play Tests

Collision tests, force tests, torque tests and stress tests can be executed by checking the respective test in the *Simulation* GameObject and pressing play.
For the description of these tests simply hover over the respective check mark on the *Simulation* GameObject.

## Known Limitations

#### Collision Margin

When the guidewire collides with a blood vessel object, Unity's contact point of the collision is not on the surface of the blood vessel, but instead somewhere inside the collision manifold of the overlapping space.
In addition, Unity's computation is not that precise. Our collision response would only work correctly, if the contact point was on the surface of the blood vessel. To accomodate for this, a collision margin is introduced.
The collision margin is added to the displacement correction of the constraint solving step of the collision constraint in the direction of the collision normal.
However, this collision margin may not have been chosen large enough to prevent the guidewire from slightly invading the blood vessel (not so critical) or, in the worst case, piercing the blood vessel (critical).
In either case, increasing the collision margin helps in preventing these cases. Decreasing the time step also helps, since the collision manifold is then smaller as the collision gets detected sooner.
Both the collision margin and the time step can be changed on the *Simulation* GameObject.

#### Performance

If the simulation requires more resources than your machine can provide, the most effective way to address this is to reduce the constraint solving steps, since these cost about 99% of the computation time.
The performance also depends on the number of spheres of the guidewire. The constraint solving steps can be changed on the *Simulation* GameObject.
If the constraing solving steps need to be drastically reduced, it is recommend to increase the stiffness parameters of the constraints accordingly.
Also, multiple guidewire prefabs differing in length are available in the Prefab folder.

## Documentation

A Documentation was created using Doxygen. It is stored within this repository [here](http://gitlab.medma.uni-heidelberg.de/rviellieber/SimulatingGuidewiresInBloodVessels/blob/main/GuidewireSimulation/GuidewireSimulation/Doxygen%20Documentation).
The PDF version of the documentation can be viewed [here](http://gitlab.medma.uni-heidelberg.de/rviellieber/SimulatingGuidewiresInBloodVessels/blob/main/GuidewireSimulation/GuidewireSimulation/Doxygen%20Documentation/latex/Documentation_Simulating_Guidewires_in_Blood_Vessels.pdf).
The HTML version can be viewed by opening [this file](http://gitlab.medma.uni-heidelberg.de/rviellieber/SimulatingGuidewiresInBloodVessels/blob/main/GuidewireSimulation/GuidewireSimulation/Doxygen%20Documentation/html/index.html) in your browser.