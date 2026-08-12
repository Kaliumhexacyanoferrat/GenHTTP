// Reports the protocol the browser actually used, which is the point of the page.
const nav = performance.getEntriesByType("navigation")[0];
const note = document.createElement("p");
note.innerHTML = `This document arrived over <code>${nav ? nav.nextHopProtocol || "(unknown)" : "(unknown)"}</code>.`;
document.body.appendChild(note);
