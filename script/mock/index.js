#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const dgram = require('dgram');
const Promise = require('bluebird');
const parse = require('csv-parse/lib/sync');

const logFile = 'replay.csv';

const port = process.argv[2] || 5001;
const host = process.argv[3] || '127.0.0.1';
const uiWidth = 1280;
const uiHeight = 720;

const client = Promise.promisifyAll(dgram.createSocket('udp4'));

let frame = 0;
let data;

const build = pks => {
  let buf = Buffer.alloc(0xffff);
  let pos = 0;

  pos = buf.write('MOCK', 0, 4);
  pos = buf.writeUInt16BE(4, pos);
  pos += 2; // length

  pks.forEach(([id, ...vs]) => {
    const lp = pos;

    pos = buf.writeUInt16BE(id, pos);
    pos += 2; // length

    vs.forEach(([t, v]) => {
      if (t === 'String') {
        pos = buf.writeUInt16BE(v.length, pos);
        for (let u = 0; u < v.length; u++) {
          pos = buf.writeInt8(v[u], pos);
        }
      } else {
        pos = buf[`write${t}BE`](v, pos);
      }
    });

    buf.writeUInt16BE((pos - lp) - 4, lp + 2);
  });

  buf.writeUInt16BE(pos - 8, 6);

  return buf.slice(0, pos);
};

// Standard Normal variate using Box-Muller transform.
function randn_bm() {
    const u = 1 - Math.random();
    const v = 1 - Math.random();

    return Math.sqrt(-2.0*Math.log(u))*Math.cos(2.0*Math.PI*v);
}

const timeout = 16;
let x = uiWidth/2, xd = 1;
let y = uiHeight/2, yd = 1;
const xMargin = uiWidth/4;
const yMargin = uiHeight/4;

const getNewGazePoint = () => {
  return [
    xMargin + Math.random()*(uiWidth-xMargin*2),
    yMargin + Math.random()*(uiHeight-yMargin*2)
  ];
}

var u = 0;
const getMessage = () => {
  u++;
  if (data) {
    const r = data[u % data.length];

    x = (parseFloat(r['GazeScreenCoords.X']) / 2 + 0.5) * uiWidth;
    y = (parseFloat(r['GazeScreenCoords.Y']) / 2 + 0.5) * uiHeight;
  } else {
    if (u === 250) {
      u = 0;
      [x, y] = getNewGazePoint();
    }
  }

  return build([
    [1, 
      ['UInt32', ++frame]],
    [0x0016, 
      ['Double', ((uiWidth - x) / uiWidth)]],
    [64, 
      ['UInt16', 1],
      ['Double', 0],
      ['Double', 0],
      ['Double', 0],
      ['Double', x + randn_bm()*10],
      ['Double', y + randn_bm()*10],
      ['Double', 0],
      ['String', 'hoi']]
  ])
};

const sendFrame = () => send(getMessage());

const send = msg =>
  client.sendAsync(msg, 0, msg.length, port, host)
    .then(bytes => {
      if (frame % 100 === 0) {
        process.stderr.write('.');
      }
      return setTimeout(sendFrame, timeout);
    });


if (logFile) {
  try {
    const input = fs.readFileSync(path.join(__dirname, logFile), 'utf-8');
    process.stdout.write(`Reading log file for replay: ${logFile}\n`);
    data = parse(
      input,
      { columns: true }
    );
  } catch (e) {
    process.stdout.write('Could not read replay file\n');
  }
}

process.stdout.write(`Sending data to ${host}:${port}\n`);
sendFrame();
