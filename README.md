# CRM — Data Interceptor for Fruit Cutting Machines (ATLAS)

> A Windows Forms utility that live-monitors, parses, and differentiates production data emitted by ATLAS fruit-cutting machines.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [How It Works](#how-it-works)
- [Data Fields](#data-fields)
- [UI Components](#ui-components)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Build & Run](#build--run)
  - [Usage](#usage)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Author](#author)
- [License](#license)

---

## Overview

**CRM** is the repository name; the application itself is labelled **CMR** (*C*utting *M*achine *R*eader / Reporter) in the source code. It is a lightweight Windows desktop application written in C# (.NET / WinForms), designed to intercept and interpret the tab-delimited data file (`current.txt`) that ATLAS fruit-cutting machines write to disk, and to present both the **raw** readings and **minute-by-minute differences** (compensated data) in a clear, real-time interface.

The tool is especially useful on the production floor where operators need to monitor multiple machine heads simultaneously and quickly spot changes in throughput, efficiency percentages, and machine cycles — without having to manually compare log files.

---

## Features

- 📂 **File browser** — select the machine's output file (`current.txt`) via a standard open-file dialog.
- ⏱️ **Timer-based polling** — automatically re-reads the file at each timer tick, detecting new data by comparing timestamps.
- 📊 **Raw data view** — displays the most recent snapshot from every machine head.
- 🔄 **Compensated (differential) data view** — shows the *change* since the previous reading for every numeric metric, making it easy to see per-minute production rates.
- 💾 **XML state persistence** — stores the previous snapshot in `predata.xml` so compensated values survive across individual timer ticks.
- 🖥️ **Multi-head support** — handles an arbitrary number of machine heads in a single file; each head is indexed automatically.
- 🔒 **Safe file access** — opens the monitored file with `FileShare.ReadWrite` so the machine software can continue writing while CRM reads.

---

## How It Works

```
┌────────────┐      timer tick       ┌──────────────────┐
│ current.txt│ ──────────────────▶  │  Parse tab data   │
│ (machine   │                       │  (all heads)      │
│  output)   │                       └────────┬─────────┘
└────────────┘                                │
                                              ▼
                               ┌──────────────────────────┐
                               │  Compare with predata.xml │
                               │  (previous minute values) │
                               └────────────┬─────────────┘
                                            │
                              ┌─────────────┴──────────────┐
                              ▼                             ▼
                     Raw data TextBox          Compensated data TextBox
                     (current snapshot)        (delta since last tick)
```

1. **Startup** — `xmlLinqCreator()` creates (or overwrites) a blank `predata.xml` with an `<Entries>` root node.
2. **Open file** — the user browses to the machine's `current.txt`.
3. **Start** — the WinForms `Timer` is enabled. On every tick, `timer1_Tick` fires:
   - Reads the first line of the file to extract the **timestamp**.
   - Skips the update if the timestamp hasn't changed since the last tick (no new data yet).
   - Iterates over every subsequent line (one per machine head), parsing 11 tab-separated fields.
   - Calls `xmlAppender()` for each head, which:
     - On the **first reading** for that head: writes the values to `predata.xml` and displays zeroed compensated fields (self-subtraction).
     - On **subsequent readings**: subtracts the stored XML values from the fresh values to produce the compensated delta, then updates the XML entry with the fresh values.
4. **Stop** — the timer is disabled, state is reset, and the UI is unlocked for the next session.

---

## Data Fields

Each line in `current.txt` (after the timestamp header) contains 11 tab-separated fields per machine head:

| # | Code | Full Name | Description |
|---|------|-----------|-------------|
| 0 | `F` | Fruit | Fruit type being processed |
| 1 | `NN` | Network Number | Machine identifier on the network |
| 2 | `MS` | Machine State | Current operational state of the machine |
| 3 | `CPF` | Current Feed Percent | Feed efficiency as a percentage |
| 4 | `CPS` | Current Spoon Percent | Spoon efficiency as a percentage |
| 5 | `CPR` | Current Rework Percent | Rework rate as a percentage |
| 6 | `PSLH` | Peaches Since Last HH File Written | Total peaches processed since last hourly file |
| 7 | `MCSLH` | Machine Cycles Since Last HH File Written | Total machine cycles since last hourly file |
| 8 | `SPSLH` | Spoon Peaches Since Last HH File Written | Spoon peaches since last hourly file |
| 9 | `RPSLH` | Rework Peaches Since Last HH File Written | Rework peaches since last hourly file |
| 10 | `MRTSLH` | Machine Runtime Since Last HH File Written | Machine runtime (seconds) since last hourly file |

> The **compensated** versions of fields 3–10 show the *difference* between the current reading and the previous one, effectively giving you per-interval production rates.

---

## UI Components

| Control | Description |
|---------|-------------|
| **Open File Button** | Opens a file dialog to browse to `current.txt` |
| **File Path Text Box** | Displays the full path of the selected file |
| **Start Button** | Enables the timer and begins polling |
| **Stop Button** | Stops the timer and resets the session |
| **Exit Button** | Closes the application |
| **Data Output Text Box** | Scrollable log of raw machine-head data |
| **Compensated Data Text Box** | Scrollable log of delta (differentiated) data |
| **Time of Last Capture Label** | Shows the timestamp of the most recently processed snapshot |
| **Number of Heads Label** | Shows how many machine heads were detected in the last read |

---

## Getting Started

### Prerequisites

- **Windows** (the application uses WinForms and is Windows-only)
- **.NET Framework** 4.x or later (or a compatible .NET version that supports `System.Windows.Forms`)
- **Visual Studio 2019 / 2022** (recommended) — or any IDE / build tool that supports .NET WinForms projects

### Build & Run

1. Clone the repository:
   ```bash
   git clone https://github.com/ChrisKarayann/crm.git
   cd crm
   ```
2. Open the solution (`.sln`) file in Visual Studio.
3. Build the solution (`Ctrl+Shift+B` or **Build → Build Solution**).
4. Run with `F5` or by executing the compiled `.exe` from the `bin/` folder.

> **Note:** `predata.xml` is created automatically in the application's working directory on every startup. You do not need to create it manually.

### Usage

1. Launch the application.
2. Click **Open File** and navigate to the `current.txt` file produced by the ATLAS machine(s).
3. Click **Start** — the tool begins polling the file.
   - The **Data Output** panel shows each machine head's current values.
   - The **Compensated Data** panel shows minute-by-minute deltas.
   - The **Time of Last Capture** label updates whenever new data is detected.
   - The **Number of Heads** label shows how many heads are active.
4. Click **Stop** to pause monitoring (you can Start again afterwards with the same or a different file).
5. Click **Exit** to close the application.

---

## Project Structure

```
crm/
├── Form1.cs        # Main WinForms form — all application logic
├── README.md       # This file
└── LICENSE         # License information
```

---

## Tech Stack

| Technology | Role |
|------------|------|
| C# | Application language |
| .NET WinForms | UI framework |
| `System.IO.StreamReader` | Non-locking file reading |
| `System.Xml.Linq` (LINQ to XML) | XML state persistence (`predata.xml`) |
| WinForms `Timer` | Periodic file polling |

---

## Author

**Chris Karayannidis** — 2021

---

## License

This project is licensed under the terms found in the [LICENSE](LICENSE) file.

