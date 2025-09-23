import stat
import subprocess
import os
import shutil
import tarfile

COPIED_FILES = [".json"]
PACKAGED_DIR = "Packaged"

ARCHIVE_DIR = "Archives"

CMD_SCRIPT = "dotnet exec/{NAME}.dll > nul"
SH_SCRIPT = "dotnet exec/{NAME}.dll"

subdirs = []

with os.scandir(".") as d:
    for item in d:
        if os.path.exists(f"./{item.name}/{item.name}.json"): # Check if this is a bot by seeing if it has the bot config file
            subdirs.append(item.name);

print(f"Detected projects: {subdirs}")

assert len(subdirs) != 0, "No bots found in working directory!"

if not os.path.exists(PACKAGED_DIR):
    os.mkdir(PACKAGED_DIR)

with os.scandir(PACKAGED_DIR) as d:
    for item in d:
        path = f"{PACKAGED_DIR}/{item.name}"
        if os.path.exists(f"{path}/{item.name}.json"):
            shutil.rmtree(path)

for subdir in subdirs:
    print(f"Creating packaged directory for {subdir}...")

    path = f"{PACKAGED_DIR}/{subdir}"
    os.mkdir(path)

    with os.scandir(subdir) as d:
        for item in d:
            copy = False
            for extension in COPIED_FILES:
                if item.name.endswith(extension):
                    copy = True
                    break
            if copy:
                print(f"Copying file {item.name}...")
                shutil.copyfile(f"./{subdir}/{item.name}", f"{path}/{item.name}")
    
    print(f"Compiling C# for {subdir}...")
    subprocess.run(['dotnet', 'publish', '-c', 'Release', '-o', f'{path}/exec', subdir], check=True)

    print(f"Creating run scripts for {subdir}...")

    for script_data in [(CMD_SCRIPT, "cmd"), (SH_SCRIPT, "sh")]:
        file_path = f"{path}/{subdir}.{script_data[1]}"
        with open(file_path, "w") as archive:
            archive.write(script_data[0].replace("{NAME}", subdir))

    print(f"Finished packaging {subdir}.")

print("Packaged all bots. Now archiving and removing directory...")

def tar_perms_filter(tarinfo: tarfile.TarInfo):
    if tarinfo.name.endswith(".sh") or tarinfo.isdir():
        tarinfo.mode = 0o755
    else:
        tarinfo.mode = 0o644

    return tarinfo

def make_archives(directory: str, name: str = None):
    if name is None: name = directory.split("/")[-1]

    print(f"Archiving {name} with tar.gz")
    with tarfile.open(f"{ARCHIVE_DIR}/{name}.tar.gz", "w:gz") as tar:
        for dir_path, _dir_names, file_names in os.walk(directory):
            dir_path_relative = os.path.relpath(dir_path, directory) # Don't nest everything inside a 'packaged' dir in the archive
            dir_path_archive = dir_path_relative.replace(os.path.sep, "/")
            if dir_path_relative != ".":
                tar.add(dir_path, dir_path_archive, recursive=False, filter=tar_perms_filter)

            for file_name in file_names:
                file_path = os.path.join(dir_path, file_name)
                file_path_archive = f"{dir_path_archive}/{file_name}"
                tar.add(file_path, file_path_archive, filter=tar_perms_filter)

    print(f"Archiving {name} with zip")
    shutil.make_archive(f"{ARCHIVE_DIR}/{name}", 'zip', directory)

if os.path.exists(ARCHIVE_DIR):
        # Probably best to clear out the dir so all files are guaranteed up-to-date
        shutil.rmtree(ARCHIVE_DIR)

os.mkdir(ARCHIVE_DIR)

make_archives(PACKAGED_DIR, "FlameFishRobocode")
for bot in os.listdir(PACKAGED_DIR):
    make_archives(f"{PACKAGED_DIR}/{bot}")

print("Removing.")
shutil.rmtree(PACKAGED_DIR)
