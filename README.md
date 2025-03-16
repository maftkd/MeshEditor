# Toy Mesh Editor for Unity Editor

![editFast](https://github.com/user-attachments/assets/e73d8cab-b840-4b74-bfc4-582d4928d645)

A toy Mesh Editor made to explore the world of CAD. The editor works exclusively within the Unity Engine and is essentially a feature-limited clone of Blender. Before starting this project, I thought it would be so simple as to not be worth my time. Upon starting it, I realized how wrong I was.

For starters, creating a "Mesh" to be sent to the gpu usually consists of vertices, triangles, and various per-vertex data. However, when manipulating mesh data in the world of cad, one must distinguish between primitives used for rendering, and those used for editing. I.e. the user need not be aware that their quad is actually two triangles.

Furthermore, to the user a vertex is just a vertex. But those who have worked with flat-shaded geometry understand that there are often multiple vertices with the same position and different normals. Of course the user for a CAD program doesn't care about these subtleties.

There have been a lot of other learnings like how to detect which primitives a user clicks on when they click to select (hint: I lean heavily on IQ's [ray-intersect](https://iquilezles.org/articles/intersectors/) formulas)

It also seems that the way Blender does box-select is by rasterizing the primitives with ID's, and that image is then passed to the CPU and analyzed pixel by pixel. At least that's the way I plan to impliment it. You can see a full list of planned features below.

## Features
- [x] Multiple selection modes (vertex, edge, face)
- [x] Translation (with gizmo & 'G' key)
- [x] Deletion
- [x] Duplication
- [x] Click selection
- [x] Undo / Redo Stack
- [ ] Box selection
- [ ] Extrude
- [ ] Inset
- [ ] Bevel
- [ ] Export

## Goals
The main goal of this project, in addition to simply learning how something like Blender works, is to produce a 3D model and print it out. When that happens, I'll drop a picture of the print below.
