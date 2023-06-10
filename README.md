<h1 align="center">Tomorrowland Session Planner</h1>

<p align="center">
  [TMLSessionPlaner-Logo](https://github.com/Json-exe/TomorrowlandSessionPlanner/assets/96955704/0e6973a4-cc19-4247-a05e-967eafe6427a)
</p>

<p align="center">
  A TML Session Planner for viewing and planning sessions at the Tomorrowland Festival.
</p>

## Features!


- View sessions easily
- Filter sessions by stage, DJ, and weekend
- Session Analyzer: Check your plan for overlapping sessions
- Session Export: HTML and TMLPlanner file formats (PDF support coming soon)
- Import plans for editing
- Clean and intuitive [MudBlazor](https://github.com/MudBlazor/MudBlazor) design
- Easy to use

## Installation

### Self-hosting

1. Download the repository.
2. Open it with Visual Studio or JetBrains Rider.
3. Modify the database connection string in the `applicationsettings.json` file.
4. Build the project.
5. Enjoy using the Tomorrowland Session Planner!

### Common Use

Visit the official Tomorrowland Session Planner website: [https://your-website-url.com](https://your-website-url.com) and start planning your sessions!

## Usage

### Common Use

Follow the steps below to create your plan:

1. Visit [https://your-website-url.com](https://your-website-url.com).
2. Follow the guided setup to create your plan.

### Self-hosting

The plan creation flow remains the same for self-hosting. To edit the database, follow these steps:

1. Go to `Pages -> Edit -> EditData.razor.cs`.
2. Change `IsActive` to `true` to activate the Editor.
3. Access the Editor using the URL `/EditData`.

## Screenshots

<p align="center">
  <img src="https://your-screenshot-url.com" alt="Screenshot 1" width="600">
</p>

<p align="center">
  <img src="https://your-screenshot-url.com" alt="Screenshot 2" width="600">
</p>

## Technologies Used

- C#
- [.NET](https://dotnet.microsoft.com/)
- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [MudBlazor](https://github.com/MudBlazor/MudBlazor) (UI)
- SQLite

## Contributing

Contributions are welcome! To contribute to the Tomorrowland Session Planner project, please follow these common guidelines:

- Fork the repository.
- Create a new branch for your feature or bug fix.
- Make your changes and commit them with descriptive messages.
- Push your changes to your forked repository.
- Submit a pull request, explaining your changes and their purpose.

We appreciate your contributions and will review them as soon as possible!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Author

- Json_exe
