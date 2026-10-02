import os
import sys
import subprocess
print ('argument list', sys.argv)
# name = sys.argv[1]

def setup():
    subprocess.call([sys.executable, '-m', 'pip', '-q', 'install', '--upgrade', 'pip'])      # -m uses pip for specified python; -q disables log
    subprocess.call([sys.executable, '-m', 'pip', 'install', '-r', 'requirements.txt', '--no-warn-script-location']) # -m uses pip for specified python; -q disables log; -r uses requirements file


setup()

print("!!! Installed requirements successfully !!!")
