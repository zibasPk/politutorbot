import React, { useState, useRef } from 'react';


import Overlay from 'react-bootstrap/Overlay';
import { Tooltip } from 'react-bootstrap';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import HelpIcon from '@mui/icons-material/Help';
import Button from 'react-bootstrap/Button';

export default function InfoIconBis(props) {
  const [show, setShow] = useState(false);
  const target = useRef(null);
  const content = props.content;
  return (
    <>
      <ClickAwayListener onClickAway={() => setShow(false)}>
        <>
        <HelpIcon ref={target} className="helpIcon" onClick={() => setShow(!show)} />
        <Overlay target={target.current} show={show} placement="top">
          {(props) => (
            <Tooltip id="overlay-example" className='largeToolTip' {...props}>
              {content}
            </Tooltip>
          )}
        </Overlay>
        </>
      </ClickAwayListener>

    </>
  );
};