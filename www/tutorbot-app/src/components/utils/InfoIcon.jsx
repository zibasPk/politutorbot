import React, { useState, useRef } from 'react';


import Tooltip from '@mui/material/Tooltip';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import HelpIcon from '@mui/icons-material/Help';

export default function InfoIcon(props) {
  const [open, setOpen] = React.useState(false);

  const handleTooltipClose = () => {
    setOpen(false);
  };

  const handleTooltipOpen = () => {
    setOpen(true);
  };

  return (
    <ClickAwayListener onClickAway={handleTooltipClose}>

      <Tooltip
        PopperProps={{
          disablePortal: true,
        }}
        onClose={handleTooltipClose}
        open={open}
        disableFocusListener
        disableHoverListener
        disableTouchListener
        title={props.text}
        className="tooltop"
      >
        <HelpIcon className="helpIcon" onClick={handleTooltipOpen} />
      </Tooltip>

    </ClickAwayListener>
  );
}

