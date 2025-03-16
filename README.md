# Toy Mesh Editor for Unity Editor

![editFast](https://github.com/user-attachments/assets/e73d8cab-b840-4b74-bfc4-582d4928d645)

A toy Mesh Editor made to explore the world of CAD. The editor works exclusively within the Unity Engine and is essentially a feature-limited clone of Blender. Before starting this project, I thought it would be so simple as to not be worth my time. Upon starting it, I realized how wrong I was.

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

## Data Structures
What is interesting about working in CAD is that the data structures for editing do not necessarily match those for rendering. For example a user sees a single quad mesh, but the GPU treats this as two triangles. The data structures are similar (if not identical to Blender's)
1. Vertex - A single Vector3
2. Edge - Two Vertex objects
3. Loop - A _starting_ vertex and a _connecting_ edge
4. Polygon - Loop start index, number of loops

## Rendering
Rendering is currently entirely done with low-level GL drawing commands like:
```
GL.Begin(GL.Lines);
for line in lines
  GL.Vertex(line.a);
  GL.Vertex(line.b);
GL.End();
```
The advantage of this is that we don't have to worry about creating triangle index arrays, and constructing Mesh objects. We don't worry about detecting changes in the geometry, we simply run the same rendering every frame.
