# Dijkstra Graph Visualizer

## About The Project
This project is a C# desktop application that visualizes Dijkstra's algorithm on a graph. The application was developed as a course assignment and demonstrates both the algorithm itself and a simple use case.

In this implementation:
- vertices represent fantasy cities
- edges represent connections between cities
- edge weights represent travel distance
- the user can choose a target city with a left click
- the user can place or remove blocked cities (walls) with a right click
- the application calculates and visualizes the shortest valid path from the start city to the selected target

The project was implemented without using any external algorithm library. Dijkstra's algorithm was written manually.

### Built With
- C#
- .NET
- Avalonia UI
- Visual Studio Code / .NET CLI

## Getting Started
These instructions explain how to install and run the program locally.

### Prerequisites
Make sure the following software is installed:
- .NET SDK
- A code editor such as Visual Studio Code
- A supported desktop OS such as Windows, Linux, or macOS

### Installation
1. Extract the project zip file.
2. Open the project folder in Visual Studio Code or a terminal.
3. Restore dependencies:

   dotnet restore

4. Build the project:

   dotnet build


### Usage
Run the application with:

 dotnet run


After starting the application:
1. Enter the number of vertices.
2. Enter the number of neighbors.
3. Click **Generate Graph**.
4. Left click a city to select it as the target.
5. Right click a city to turn it into a wall or remove the wall.
6. The program visualizes Dijkstra's algorithm and highlights the shortest valid path.
7. Click **Clear Walls** to remove all walls.

## Roadmap
### Implemented
- Graph generation with vertices and weighted edges
- Manual implementation of Dijkstra's algorithm
- Graph visualization with Avalonia
- Start node and selectable target node
- Animated traversal of active edges
- Shortest-path highlighting
- Right-click wall placement
- Fantasy city naming as a use case
- Automatic graph scaling to fit the window

### Possible Future Improvements
- Drag-and-drop vertex placement
- Better edge labels for displayed distances
- Dedicated legend for node colors
- Export of graph screenshots
- Optional grid-based map version

## Contributing
This project was created as a university assignment. It is not intended as an actively maintained public project, but ideas and improvements are welcome.

## License
This project is submitted for academic purposes.

## Contact
Name: Timon Schneider
Student ID: cc241026

## Acknowledgments
- Course materials and lecture examples
- DataStructureLibrary / Graph structures used in the course
- Dijkstra's original shortest-path concept (WIKI)
