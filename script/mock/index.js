#!/usr/bin/env node

const Promise = require('bluebird');
const dgram = require('dgram');

const port = process.argv[2] || 5001;
const host = process.argv[3] || '127.0.0.1';
const uiWidth = 1280;
const uiHeight = 720;

const client = Promise.promisifyAll(dgram.createSocket('udp4'));

let frame = 0;

const build = pks => {
  let buf = Buffer.alloc(0xffff);
  let pos = 0;

  pos = buf.write('MOCK', 0, 4);
  pos = buf.writeUInt16BE(1234, pos);
  pos += 2; // length

  pks.forEach(([id, ...vs]) => {
    const lp = pos;

    pos = buf.writeUInt16BE(id, pos);
    pos += 2; // length

    vs.forEach(([t, v]) => {
      pos = buf[`write${t}BE`](v, pos);
    });

    buf.writeUInt16BE((pos - lp) - 4, lp + 2);
  });

  buf.writeUInt16BE(pos, 6);

  // console.log(buf.slice(0, pos));
  return buf.slice(0, pos);
};

const timeout = 25;
let x = 0, xd = 1;
let y = 0, yd = 1;
const margin = 50;

const getMessage = () => {
  x = x + xd * (Math.random() * timeout/2);
  if (x > uiWidth - margin) {
    xd = -1;
    x = uiWidth - margin;
  } else if (x < margin) {
    xd = 1;
    x = margin;
  }

  y = y + yd * (Math.random() * timeout/2);
  if (y > uiHeight - margin) {
    yd = -1;
    y = uiHeight - margin;
  } else if (y < margin) {
    yd = 1;
    y = margin;
  }

  return build([
    [1, 
      ['UInt32', ++frame]],
    // [0x0016, 
    //   ['Double', ((uiWidth - x) / uiWidth)]],
    [64, 
      ['UInt16', 1],
      ['Double', 0],
      ['Double', 0],
      ['Double', 0],
      ['Double', x],
      ['Double', y],
      ['Double', 0]]
  ])
};

const sendFrame = () => send(getMessage());

const send = msg =>
  client.sendAsync(msg, 0, msg.length, port, host)
    .then(bytes => {
      console.log(frame, x, y);
      return setTimeout(sendFrame, timeout);
    });

sendFrame();
