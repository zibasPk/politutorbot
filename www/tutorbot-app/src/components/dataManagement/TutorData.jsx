import React, {useState } from 'react';

import styles from './DataManagement.module.css'
import configData from "../../config/config.json";

import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';

import DeleteButton from '../utils/DeleteButton';

export default function TutorData()
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
        <h1>Gestione Dati Tutor{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit className={styles.tutorDataCont + " contentWithBg"}>
          <div>Usa questa funzionalità per eliminare tutti i tutor e tutoraggi dal sistema</div>
          <DeleteButton btnText='Reset dati Tutor e Tutoraggi' 
          modalTitle='Eliminazione Tutor e Tutoraggi' 
          deleteEndpoint={configData.botApiUrl + '/tutors/'}/>
        </Collapse>
      </div>
    </>
  );
}