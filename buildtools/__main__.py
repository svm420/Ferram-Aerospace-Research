#!/usr/bin/env python3
# -*- coding:utf-8 -*-
"""
@Author:               Daumantas Kavolis <dkavolis>
@Date:                 05-Apr-2019
@Filename:             __main__.py
@Last Modified By:     Daumantas Kavolis
@Last Modified Time:   05-Apr-2019
"""


import argparse
from buildtools import replace, package, postbuild, common


def main():
    parser = argparse.ArgumentParser("C# build helpers", add_help=False)

    subparsers = parser.add_subparsers(description="Available helpers", dest="command")

    common.add_config_option(parser)
    replacer = subparsers.add_parser(
        "replace", description="Regex replacement utility", parents=[parser]
    )
    packager = subparsers.add_parser(
        "package", description="Archive utility", parents=[parser]
    )
    post = subparsers.add_parser(
        "postbuild", description="Post build utility", parents=[parser]
    )

    # args.command always contains None so add defaults
    replace.build_parser(replacer)
    replacer.set_defaults(run=replace.run)

    package.build_parser(packager)
    packager.set_defaults(run=package.run)

    postbuild.build_parser(post)
    post.set_defaults(run=postbuild.run)

    args = parser.parse_args()

    config = common.load_config(args.config)

    with common.chdir(config["root"]):
        if hasattr(args, "run"):
            args.run(config, args)
        else:
            parser.print_help()


if __name__ == "__main__":
    main()
