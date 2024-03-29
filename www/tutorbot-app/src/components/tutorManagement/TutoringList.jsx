import React, { useEffect, useState } from 'react';

import styles from './TutorManagement.module.css'
import configData from "../../config/config.json";

import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import { CircularProgress } from '@mui/material';
import Table from '../utils/Table';
import TutoringListModal from './TutoringListModal';


export default function TutoringList(props)
{
  const [expanded, setExpanded] = useState(true);

  const handleExpandClick = () =>
  {
    setExpanded(!expanded);
  };

  const icon = !expanded ? <ExpandMoreIcon
    expand={expanded.toString()}
    onClick={handleExpandClick}
    aria-expanded={expanded}
    aria-label="show more"
    fontSize='none'
    className={styles.btnExpand}
  /> : <ExpandLessIcon
    expand={expanded.toString()}
    onClick={handleExpandClick}
    aria-expanded={expanded}
    aria-label="show more"
    fontSize='none'
    className={styles.btnExpand}
  />;

  return (
    <>
      <div className={styles.dropDownContent}>
        <h1>Lista Tutoraggi{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <div className={styles.tutorListContent + " contentWithBg"}>
            {props.tutoringList != undefined?
              <Table headers={props.headers} content={props.tutoringList} hasChecks={true}
                modalProps={{
                  modalTitle: "Elimina Tutoraggi selezionati",
                  modalContent: TutoringListModal,
                  contentHeaders: props.headers,
                  onModalEvent: props.refreshData
                }}
              /> :
              <div className='pendingDiv'><CircularProgress /></div>}
          </div>
        </Collapse>
      </div>
    </>
  );
}