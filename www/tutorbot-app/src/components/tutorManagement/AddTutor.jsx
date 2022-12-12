import React, { useEffect, useState } from 'react';

import styles from './TutorManagement.module.css'
import configData from "../../config/config.json";
import pic from '../../assets/excel-pic.png';

import Form from 'react-bootstrap/Form';
import { Button } from 'react-bootstrap';
import InfoIconBis from '../utils/InfoIconBis';
import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';


export default function AddTutor()
{
  const [courses, setCourses] = useState([])
  const [expanded, setExpanded] = useState(true);

  useEffect(() =>
  {
    fetch(configData.botApiUrl + '/course', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((courses) => {
        courses.sort((a,b) => a.localeCompare(b));
        setCourses(courses);
      })
  }, [])

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
        <h1>Aggiungi Tutor{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <div className={styles.addTutorContent}>
            <div className={styles.newTutorForm}>
              <Form>
                <Form.Group controlId="formTutorCode" className="mb-3">
                  <Form.Label>Matricola Tutor</Form.Label>
                  <Form.Control type="text" placeholder="Matr. Studente" />
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label>Corso di studi</Form.Label>
                  <Form.Select>
                    {courses.map((course) => <option key={course}>{course}</option> )}
                  </Form.Select>
                </Form.Group>
                <Form.Group controlId="formTutorName" className="mb-3">
                  <Form.Label>Nome Tutor</Form.Label>
                  <Form.Control type="text" placeholder="Nome" />
                </Form.Group>
                <Form.Group controlId="formTutorSurname" className="mb-3">
                  <Form.Label>Cognome Tutor</Form.Label>
                  <Form.Control type="text" placeholder="Cognome" />
                </Form.Group>
                <Form.Group controlId="formExamCode" className="mb-3">
                  <Form.Label>Esame</Form.Label>
                  <Form.Control type="text" placeholder="Codice Esame" />
                </Form.Group>
                <Form.Group controlId="formExamCode" className="mb-3">
                  <Form.Label>Posizione in Graduatoria</Form.Label>
                  <Form.Control type="text" placeholder="Posizione" />
                </Form.Group>
                <Form.Group className="mb-3" controlId="formOFACheckbox">
                  <Form.Check type="checkbox" label="disponibile per OFA" />
                </Form.Group>
                <Button className={styles.addButton} variant="warning" type="submit">
                  Aggiungi
                </Button>
              </Form>
            </div>
            <Form.Group controlId="formTutorFile" className="mb-3">
              <Form.Label>Carica File CSV Tutor</Form.Label>
              <InfoIconBis content={<>
                <div>Inserire un file cvs con righe come da figura:</div>
                <img src={pic}></img>
              </>} />
              {/* <InfoIcon text={<img src={pic}></img>} /> */}
              <Form.Control type="file" />
            </Form.Group>
          </div>
        </Collapse>
      </div>
    </>
  );
}