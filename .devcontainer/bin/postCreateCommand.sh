#!/bin/bash

set -ev

sudo apt update -qq
sudo apt install -qq -y vim

git config core.editor "vim"
