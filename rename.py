import os
import fileinput
import sys

# If a line contains any of these words, ignore it
blacklist = ["Copyright", "bitcoin.org", "github.com", "COPYRIGHT"]

for filename in os.listdir(os.getcwd()):
    if os.path.isdir(filename) or ".bak" in filename or ".png" in filename or ".dat" in filename or ".raw" in filename or ".ico" in filename or ".icns" in filename or ".svg" in filename:
        pass
    else:
        print("Replacing in file " + filename, file=sys.stderr)
        with fileinput.FileInput(filename, inplace=True, backup='.bak') as file:
            for line in file:
                if not any(x in line for x in blacklist):
                    line = line.replace("bitcoincore.org", "gainzcoin.org")
                    line = line.replace("bitcoin", "gainzcoin")
                    line = line.replace("Bitcoin", "GainzCoin")
                    line = line.replace("BITCOIN", "GAINZCOIN")

                print(line, end='')

    orig_filename = filename
    filename = filename.replace("bitcoin", "gainzcoin")
    os.rename(orig_filename, filename)
