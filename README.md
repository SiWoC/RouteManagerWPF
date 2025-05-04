# RouteManagerWPF

A Windows desktop application for building and converting routes for Road Trips.

## Features

- Import and export routes in multiple formats:
  - TRP (CoPilot) files
  - GPX files
  - VCF (vCard) files for Volkswagen RNS510 navigation system
- Interactive map interface with Google Maps and OpenStreetMap support
- Route point management:
  - Add, delete, and reorder points
  - Drag and drop points on the map
  - Automatic road snapping
- Route statistics:
  - Total distance
  - Estimated duration
- OpenStreetMap or Google Routing (Google needs an API key)

## Requirements

- Windows 10 or later
- .NET 6.0 or later
- Google Maps API key (optional, for Google Maps provider)

## Installation

1. Download the latest release from the [Releases](https://github.com/SiWoC/RouteManagerWPF/releases) page
2. Extract to a folder of your liking
3. Launch RouteManager.exe

## Usage

### Creating a New Route

1. Click "New Route" or press Ctrl+N
2. Add points by:
   - Right-clicking on the map
   - Using the "Add Point" button
3. Points are inserted after the currently selected point
4. Reorder points by dragging them in the list
5. Save your route using "Save" or Ctrl+S

### Exporting Routes

1. Click "Save Route" or press Ctrl+S
2. Choose the desired format:
   - TRP/GPX: Select a file location
   - VCF: Select a folder to save multiple VCF files

## VCF Format Notes

- VCF files are created in a destionations subfolder as you would need to import in the RNS510
- Special characters in names are properly escaped
- Filenames = POI names follow a pattern of
  - Routename or Routeprefix
  - Index/OrderNumber
  - Name
For importing these POI's and creating a route please check out the [VCFCreator docs](https://github.com/SiWoC/VcfCreator/blob/master/src/resources/Handleiding%20-%20Ritten%20en%20de%20RNS%20510.docx)

## Known issues
- The traffic toggle for showing the Google Maps Traffic overlay has trouble with caching. But I leave it for now, because it does show you why Google won't put your route over a closed section.

## License

This project is licensed under the GNU General Public License v3 (GPLv3) - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. 