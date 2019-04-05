#!/usr/bin/env python3
# -*- coding:utf-8 -*-
"""
@Author:               Daumantas Kavolis <dkavolis>
@Date:                 05-Apr-2019
@Filename:             replace.py
@Last Modified By:     Daumantas Kavolis
@Last Modified Time:   05-Apr-2019
"""

from __future__ import annotations

import argparse
import glob
import re

from buildtools import common


def run(config, args):
    replace(config)


def build_parser(parser):
    pass


def replace(config):
    for glob_pattern, subs in config["replace"].items():
        files = common.glob(common.resolve_path(glob_pattern, config))

        for filename in files:
            replace_in_file(filename, subs, config)


def replace_in_file(filename, replacements, config):
    print(f"Updating {filename!r}")
    with open(filename, "r") as file:
        contents = file.read()
    for pattern, replacement in replacements.items():
        pattern = common.resolve_path(pattern, config)
        replacement = common.resolve_path(replacement, config)

        def replace(matchobj):
            if (len(matchobj.groups())) > 0:
                string = matchobj[0]
                start_, end_ = matchobj.start(), matchobj.end()
                start, end = matchobj.start(1), matchobj.end(1)
                new = string[: start - start_] + replacement
                if end != end_:
                    new = new + string[end - end_ :]
                return sub_groups(new, (matchobj[0],) + matchobj.groups())
            return sub_groups(replacement, (matchobj[0],) + matchobj.groups())

        contents = re.sub(pattern, replace, contents)
    with open(filename, "w") as file:
        file.write(contents)


def main():
    parser = argparse.ArgumentParser(description="Regex replacement utility")
    common.add_config_option(parser)
    build_parser(parser)

    args = parser.parse_args()

    config = common.load_config(args.config)
    with common.chdir(config["root"]):
        replace(config)


GROUP = re.compile(r"(?:\\g<(\d+)>|\\(\d+))")


def sub_groups(string, groups):
    def _replace_group(matchobj):
        index = int(matchobj[1])
        if index < len(groups):
            return str(groups[index])
        return matchobj[0]

    return re.sub(GROUP, _replace_group, string)


if __name__ == "__main__":
    main()
