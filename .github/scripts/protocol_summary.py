#!/usr/bin/env python3
"""
Summarizes the TRX results of an acceptance test run against ioxide (analysis only).

Only tests that actually ran a server on ioxide are counted: those whose output carries the
"[test-host] engine=Ioxide" marker written by TestHost, or whose display name was generated
for the ioxide engine by the multi engine attributes.

Usage: protocol_summary.py <protocol> <results.trx> <summary.md> <failures.json>
"""

import json
import re
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

MARKER = "[test-host] engine=Ioxide"


def text(node, path):
    found = node.find(path, NS)
    return found.text if found is not None and found.text else ""


def main(protocol, trx, summary_path, failures_path):
    root = ET.parse(trx).getroot()

    classes = {}

    for definition in root.iterfind("t:TestDefinitions/t:UnitTest", NS):
        method = definition.find("t:TestMethod", NS)
        classes[definition.get("id")] = method.get("className") if method is not None else ""

    counts = {"Passed": 0, "Failed": 0, "Other": 0}
    skipped = 0
    failures = []

    for result in root.iterfind("t:Results/t:UnitTestResult", NS):
        name = result.get("testName")
        outcome = result.get("outcome")
        stdout = text(result, "t:Output/t:StdOut")

        if MARKER not in stdout and "(Ioxide" not in name:
            skipped += 1
            continue

        key = outcome if outcome in ("Passed", "Failed") else "Other"
        counts[key] += 1

        if key != "Passed":
            server_log = [line[len("[test-host] "):] for line in stdout.splitlines()
                          if line.startswith("[test-host] ") and "engine=" not in line]

            failures.append({
                "class": classes.get(result.get("testId"), ""),
                "test": name,
                "outcome": outcome,
                "message": text(result, "t:Output/t:ErrorInfo/t:Message").strip(),
                "stack": text(result, "t:Output/t:ErrorInfo/t:StackTrace").strip()[:3000],
                "server_log": server_log[:20],
            })

    total = sum(counts.values())
    rate = (100.0 * counts["Passed"] / total) if total else 0.0

    by_class = {}

    for failure in failures:
        short = re.sub(r"^GenHTTP\.Testing\.Acceptance\.", "", failure["class"])
        by_class[short] = by_class.get(short, 0) + 1

    with open(summary_path, "w", encoding="utf-8") as out:
        out.write(f"## ioxide {protocol}\n\n")
        out.write("| Tests (on ioxide) | Passed | Failed | Other | Pass rate |\n")
        out.write("|---|---|---|---|---|\n")
        out.write(f"| {total} | {counts['Passed']} | {counts['Failed']} | {counts['Other']} | {rate:.1f}% |\n\n")
        out.write(f"_{skipped} tests did not run a server on ioxide and are not counted._\n\n")

        if by_class:
            out.write("<details><summary>Failures by test class</summary>\n\n")
            out.write("| Class | Failures |\n|---|---|\n")
            for cls, count in sorted(by_class.items(), key=lambda kv: (-kv[1], kv[0])):
                out.write(f"| {cls} | {count} |\n")
            out.write("\n</details>\n")

    with open(failures_path, "w", encoding="utf-8") as out:
        json.dump({"protocol": protocol, "total": total, "counts": counts, "skipped": skipped,
                   "failures": failures}, out, indent=2)


if __name__ == "__main__":
    main(*sys.argv[1:5])
