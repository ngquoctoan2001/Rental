import os

log_file = "clean_build.log"
if os.path.exists(log_file):
    with open(log_file, "r", encoding="utf-16" if os.path.getsize(log_file) > 0 else "utf-8") as f:
        # MSBuild /fl might use UTF-16, let's try common ones
        try:
            lines = f.readlines()
        except UnicodeDecodeError:
            f.seek(0)
            lines = f.read().decode('utf-16').splitlines()
        
        errors = [line.strip() for line in lines if "error CS" in line]
        for error in errors[:10]: # Print first 10 errors
            print(error)
else:
    print(f"Log file {log_file} not found.")
