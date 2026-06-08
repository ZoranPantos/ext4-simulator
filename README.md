# Ext4 File System Simulation

A console application that simulates an **ext4-style file system inside a single binary file**, driven by a small Linux-like command shell. User creats directories and files, write and read their contents, copy/move/rename/delete them, and move data in and out of the host machine — all backed by one file on user's real disk that acts as the simulated "drive".

---

## What it does

The app opens an interactive prompt (`command> `) and lets user operate a self-contained file system:

- Create directories and files, organised under a single top-level `ROOT` directory.
- Write text into files and print it back out.
- Copy, move, rename, and delete files and directories.
- Import a real file from the host into the simulation (`put`) and export one back out (`get`).
- Inspect file metadata and i-node information, and view disk statistics.

Everything is persisted to the backing file, so the file system survives between runs.

---

## How it works

The entire file system lives in **one binary file** that is treated as a fixed-size raw disk. All reads and writes go through `BinaryReader`/`BinaryWriter` at computed byte offsets — there is no host-filesystem directory tree involved.

### On-disk layout

The 20 MB file is divided into four regions:

| Region | Byte range | Size | Contents |
|---|---|---|---|
| Metadata / superblock | `0 – 499,999` | 500 KB | Boot count, capacities, region offsets, i-node counts, free/next-write pointers, last-used i-node IDs |
| File i-node table | `500,000 – 3,499,999` | 3 MB | File i-nodes (152 bytes each) |
| Directory i-node table | `3,500,000 – 4,499,999` | 1 MB | Directory i-nodes (144 bytes each); `ROOT` is the first one |
| File data | `4,500,000 – 19,999,999` | 15.5 MB | File contents, stored in blocks |

**Total: 20,000,000 bytes (~20 MB).**

### i-nodes

- **Directory i-node (144 B):** ordinal number, ID, file count, directory count, 16 file-i-node pointers, 8 sub-directory pointers, name, parent.
- **File i-node (152 B):** name, directory, owner, creation time, block count, file size, block-address list, block-size list.

### Data blocks

File contents are stored in blocks with a **minimum block size of 5 bytes**. When data is written, a free-space scan walks the data region looking for runs of unused (zero) bytes and allocates space as `(address, length)` extents, recorded in the file's i-node. Maximum size for a single file is **64,000 bytes**.

### First launch

On the very first launch (when the backing file is brand new), the metadata region is initialised and the `ROOT` directory is created. A boot counter in the metadata region is incremented on every subsequent launch.

## Where the backing file is stored

On first launch the simulator creates a single binary file named **`FILE SYSTEM`** (no extension) in the **current working directory** — i.e. wherever the process is started from:

- `dotnet run` → the project folder.
- the built executable → its output folder, e.g. `src/Simulator/bin/Debug/net10.0/`.

This file **is** the simulated disk. It is sized to 20 MB up front and **persists between runs**, so user's directories and files are still there next time. The same folder is also used by `put` (reads a host file from here) and `get` (writes a host file to here).

> **Reset:** delete the `FILE SYSTEM` file to start from an empty file system — `ROOT` is recreated automatically on the next launch.

---

## Requirements & running

- [.NET 10 SDK](https://dotnet.microsoft.com/) or later.

```bash
# from the repository root
dotnet build "Ext4FileSystemSimulation.sln"
dotnet run --project "src/Simulator/Simulator.csproj"
```

Then type `help` at the prompt to see the available commands. The app is best used in an interactive terminal; on Windows it also sets the console title and a 70×40 window (this is skipped automatically on other platforms or when output is redirected).

---

## Commands

Paths begin at `ROOT` and use `/` as the separator (e.g. `ROOT/file` or `ROOT/dir/file`).

| Command | What it does | Example |
|---|---|---|
| `mkdir ROOT/<dir>` | Create a directory under ROOT | `mkdir ROOT/docs` |
| `create ROOT/<file>` | Create an empty file (also works inside a directory) | `create ROOT/docs/todo.txt` |
| `echo ROOT/<file> <text>` | Write `<text>` into an existing file | `echo ROOT/notes.txt hello` |
| `cat ROOT/<file>` | Print a file's contents | `cat ROOT/notes.txt` |
| `ls ROOT/` | List ROOT (use `ls ROOT/<dir>` for a directory) | `ls ROOT/docs` |
| `stat ROOT/<file>` | Show a file's attributes and i-node info | `stat ROOT/notes.txt` |
| `rename ROOT/<item> <name>` | Rename a file or directory | `rename ROOT/notes.txt n.txt` |
| `cp ROOT/<file> <dir>` | Copy a file into a directory (into ROOT: use `ROOT/`) | `cp ROOT/notes.txt ROOT/docs` |
| `mv ROOT/<file> <dir>` | Move a file into a directory (into ROOT: use `ROOT/`) | `mv ROOT/docs/todo.txt ROOT/` |
| `rm ROOT/<file>` | Delete a file (or a directory: `rm ROOT/<dir>`) | `rm ROOT/notes.txt` |
| `rm-r ROOT/` | Empty ROOT (or a directory: `rm-r ROOT/<dir>`) | `rm-r ROOT/docs` |
| `put <host_file>` | Upload a file from the working folder into ROOT | `put report.txt` |
| `get ROOT/<file>` | Download a file to the working folder | `get ROOT/notes.txt` |
| `dstat` | Print disk statistics | `dstat` |
| `help` | Show in-app help (commands + examples) | `help` |
| `clear` | Clear the screen | `clear` |
| `exit` | Quit the application | `exit` |

### Example session

Run these in order:

```text
mkdir ROOT/docs               # create directory 'docs'
create ROOT/notes.txt         # create file 'notes.txt'
echo ROOT/notes.txt hello     # write 'hello' into notes.txt
cat ROOT/notes.txt            # prints: hello
create ROOT/docs/todo.txt     # create 'todo.txt' inside 'docs'
echo ROOT/docs/todo.txt task  # write 'task' into todo.txt
ls ROOT/                      # list ROOT
ls ROOT/docs                  # list files in 'docs'
cp ROOT/notes.txt ROOT/docs   # copy notes.txt into 'docs'
mv ROOT/docs/todo.txt ROOT/   # move todo.txt to ROOT
rename ROOT/notes.txt n.txt   # rename notes.txt to n.txt
stat ROOT/n.txt               # show n.txt attributes
rm ROOT/n.txt                 # delete file n.txt
rm ROOT/docs                  # delete 'docs' and its contents
dstat                         # show disk usage
```

---

## Rules & limits

- A single top-level `ROOT` directory with **one level of subdirectories** (e.g. `ROOT/dir`); no nested subdirectories.
- Up to **8 directories** under ROOT, and up to **16 files** per directory.
- Directory and file names: **15 characters maximum**.
- Commands take **at most two** space-separated arguments, and arguments **cannot contain spaces**.
- `echo` text and `rename` names are a **single token** (no spaces, no `/`).
- When copying or moving *into* ROOT, write the destination as `ROOT/` (with the trailing slash).
- `get` will not overwrite a file that already exists in the working folder.
- Maximum file size: **64,000 bytes**.

---

