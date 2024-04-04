import sys
import vtk

def convert_vtk_to_obj(vtk_file, obj_file):
    # Read the VTK file
    reader = vtk.vtkDataSetReader()
    reader.SetFileName(vtk_file)
    reader.Update()

    # Get the output data
    output = reader.GetOutput()

    # Check if the dataset type is "unstructured_grid"
    if output.IsA("vtkUnstructuredGrid"):
        # Get the points
        points = output.GetPoints()
        num_points = points.GetNumberOfPoints()

        # Open the OBJ file for writing
        with open(obj_file, "w") as f:
            # Write the vertex positions
            for i in range(num_points):
                point = points.GetPoint(i)
                f.write(f"v {point[0]} {point[1]} {point[2]}\n")

            # Write the face indices
            cell_array = output.GetCells()
            cell_array.InitTraversal()
            cell = vtk.vtkIdList()
            while cell_array.GetNextCell(cell):
                num_cell_points = cell.GetNumberOfIds()
                if num_cell_points == 3:
                    # Write triangle face
                    f.write(f"f {cell.GetId(0)+1} {cell.GetId(1)+1} {cell.GetId(2)+1}\n")
                elif num_cell_points == 4:
                    # Write quadrilateral face as two triangles
                    f.write(f"f {cell.GetId(0)+1} {cell.GetId(1)+1} {cell.GetId(2)+1}\n")
                    f.write(f"f {cell.GetId(2)+1} {cell.GetId(3)+1} {cell.GetId(0)+1}\n")
                else:
                    print(f"Skipping face with {num_cell_points} points")

    else:
        print("Invalid dataset type. Only 'vtkUnstructuredGrid' is supported.")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Usage: python VTKTOOBJ.py <input.vtk> <output.obj>")
    else:
        vtk_file = sys.argv[1]
        obj_file = sys.argv[2]
        convert_vtk_to_obj(vtk_file, obj_file)

