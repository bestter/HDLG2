import sys

def apply_patch(file_path, patch_file):
    with open(patch_file, 'r') as f:
        patch_text = f.read()

    search_block = patch_text.split('<<<<<<< SEARCH')[1].split('=======')[0].strip('\n')
    replace_block = patch_text.split('=======')[1].split('>>>>>>> REPLACE')[0].strip('\n')

    with open(file_path, 'r') as f:
        content = f.read()

    if search_block in content:
        new_content = content.replace(search_block, replace_block)
        with open(file_path, 'w') as f:
            f.write(new_content)
        print("Patch applied successfully.")
    else:
        print("Error: Search block not found in file.")

apply_patch("HDLG winforms/HdlgDirectory.cs", "patch.diff")
