import React, { useState, useRef } from 'react';


import Overlay from 'react-bootstrap/Overlay';
import { Tooltip } from 'react-bootstrap';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import HelpIcon from '@mui/icons-material/Help';
import Button from 'react-bootstrap/Button';

export default function InfoIconBis(props) {
  const [show, setShow] = useState(false);
  const target = useRef(null);
  const text = props.text;
  return (
    <>
      <ClickAwayListener onClickAway={() => setShow(false)}>
        <>
        <HelpIcon ref={target} className="helpIcon" onClick={() => setShow(!show)} />
        <Overlay target={target.current} show={show} placement="right">
          {(props) => (
            <Tooltip id="overlay-example" className='largeToolTip' {...props}>
              {text}
            </Tooltip>
          )}
        </Overlay>
        </>
      </ClickAwayListener>

    </>
  );
};