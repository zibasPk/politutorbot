import React, { useEffect, useState } from 'react';

import styles from './TutorManagement.module.css'
import configData from "../../config/config.json";

import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import Table from '../utils/Table';


export default function TutorsList(props) {
  const [expanded, setExpanded] = useState(true);
  const [tutors, setTutors] = useState([]);

  const handleExpandClick = () => {
    setExpanded(!expanded);
  };

  useEffect(() =>
  {
    fetch(configData.botApiUrl + '/tutor', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((tutors) => {
        tutors.forEach((tutor,i) => {
          tutor.OfaAvailable ? tutor.OfaAvailable = "SI" : tutor.OfaAvailable = "NO";
          tutor.Id = i;
        });
        setTutors(tutors);
      });
  }, [])

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
        <h1>Lista Tutor{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <div className={styles.tutorListContent}>
            <Table headers={props.headers} content={props.tutorList} hasChecks={false}
              modalProps={{
                modalTitle: "Cambia stato tutor selezionati"
              }} />
          </div>
        </Collapse>
      </div>
    </>
  );
}