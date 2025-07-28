import subprocess
import os
import shutil

COPIED_FILES = [".json"]
PACKAGED_DIR = "./Packaged"

CMD_SCRIPT = "dotnet exec/{NAME}.dll>nul"
SH_SCRIPT = "#!/bin/sh \n dotnet exec/{NAME}.dll"

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
                if item.name.endswith(extension): copy = True
            
            if copy:
                print(f"Copying file {item.name}...")
                shutil.copyfile(f"./{subdir}/{item.name}", f"{path}/{item.name}")
    
    print(f"Compiling C# for {subdir}...")
    subprocess.run(f"dotnet publish -c Release -o \"{path}/exec\" {subdir}", check=True)

    print(f"Creating run scripts for {subdir}...")

    for script_data in [(CMD_SCRIPT, "cmd"), (SH_SCRIPT, "sh")]:
        with open(f"{path}/{subdir}.{script_data[1]}", "w") as file:
            file.write(script_data[0].replace("{NAME}", subdir))

    print(f"Finished packaging {subdir}.")

print("Packaged all bots. Now zipping and removing directory...")

shutil.make_archive(PACKAGED_DIR, 'zip', PACKAGED_DIR)
shutil.rmtree(PACKAGED_DIR)