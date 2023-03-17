#!/bin/bash

dir=${1:-.}
dir_name=$(basename "$dir")

# Recursive function to build the JSON tree
build_json_tree() {
    local prefix=""
    local contents=""
    for item in "$1"/*; do
        local name=$(basename "$item")
        if [ -f "$item" ] && [[ ! "$name" == *.meta ]]; then
            contents="$contents $prefix{\"type\": \"file\", \"name\": \"$name\"}"
        elif [ -d "$item" ]; then
            contents="$contents $prefix{\"type\": \"folder\", \"name\": \"$name\", \"contents\": [$(build_json_tree "$item")]}"
        fi
        prefix=", "
    done
    echo "$contents"
}
dirname=$(basename "$(pwd)")
# Build the JSON tree and save it to a file
echo "[{ \"type\": \"folder\", \"name\": \"$dir_name\", \"contents\": [$(build_json_tree "$dir")] }]" > "$dirname.json"
echo file written at: $dirname.json