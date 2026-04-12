# Report

Course: C# Development SS20?? (4 ECTS, 3 SWS)

Student ID: cc241026

BCC Group: C

Name: Timon Schneider

## Methodology
The goal of the project was to implement Dijkstra's algorithm from scratch and build a small application that demonstrates a practical use case.

I first studied the algorithm and its core idea: every vertex receives a tentative distance from the start vertex. The start vertex is initialized with distance 0, while all other vertices are initialized with infinity. During execution, the algorithm repeatedly selects the unvisited vertex with the currently smallest tentative distance, updates its neighbors, and stores the previous vertex if a shorter path is found.

For the implementation, I used a graph-based structure with vertices and edges. The graph is generated dynamically. One vertex is marked as the start node, and the user can select another vertex as the target. The shortest path is then calculated and highlighted.

To create a more meaningful use case, the abstract node names were replaced with fantasy city names. In this scenario, vertices represent cities, edges represent routes between cities, and edge weights represent travel distance. This gives the algorithm a clearer application context similar to route planning.

The visualization was implemented with Avalonia. The graph is displayed in a resizable window, and the rendering scales so that all vertices stay inside the visible area. During execution, visited nodes, active edges, blocked cities, and the final shortest path are shown in different colors.

## Additional Features
The following features were added beyond the minimum algorithm implementation:

- Interactive visualization of Dijkstra's algorithm
- Right-click wall placement to block cities
- Highlighting of visited nodes and final shortest path
- Automatic graph scaling so the graph fits the application window
- Fantasy city naming for the use case
- Clear Walls button for faster testing

These features improve usability and make the algorithm easier to demonstrate during the presentation.

## Discussion/Conclusion
One challenge was understanding that Dijkstra's algorithm should not simply search for the end node directly. Instead, it must always process the unvisited vertex with the smallest known tentative distance. This required a loop-based implementation rather than a recursive search.

Another challenge was handling blocked nodes correctly. After walls were added, the algorithm initially still reset or ignored them incorrectly. This was solved by preserving the wall state during graph resets and excluding wall vertices from the unvisited list during Dijkstra's execution.

A further challenge was visualization. Fixed scaling caused vertices to be drawn outside the visible area, especially when the graph grew larger. This was solved by computing the scale dynamically based on the current window size and the range of vertex coordinates.

In conclusion, the project fulfills the assignment requirements by implementing Dijkstra's algorithm manually, applying it to a use case, and presenting the result in an interactive graphical application.

## Work with
This project was implemented individually. Discussions with classmates may have helped with understanding concepts, but the final implementation is my own work.

## Reference
- Course lecture slides and exercises
- Course DataStructureLibrary / Graph implementation
- General documentation for C#, .NET, and Avalonia
- Edsger W. Dijkstra, shortest path algorithm on Wikipedia & Youtube
