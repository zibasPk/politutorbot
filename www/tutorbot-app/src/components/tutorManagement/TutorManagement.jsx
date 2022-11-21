import React from 'react';
import styles from './TutorManagement.module.css'
import Form from 'react-bootstrap/Form';

import { Button } from 'react-bootstrap';
import pic from '../../assets/excel-pic.png';
import InfoIconBis from '../utils/InfoIconBis';

function TutorManagement() {
  return (
    <div className={styles.content}>
      <div className={styles.functionsHeader}>
        <h1>Aggiungi Tutor</h1>
        <div className={styles.newTutorForm}>
          <Form>
            <Form.Group controlId="formTutorCode" className="mb-3">
              <Form.Label>Matricola Tutor</Form.Label>
              <Form.Control type="text" placeholder="Matr. Studente" />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Corso di studi</Form.Label>
              <Form.Select>
                <option>Aereospaziale</option>
                <option>Informatica</option>
                <option>Gestionale</option>
                <option>Automazione</option>
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
            <Form.Group className="mb-3" controlId="formOFACheckbox">
              <Form.Check type="checkbox" label="disponibile per OFA" />
            </Form.Group>
            <Button className={styles.addButton} variant="warning" type="submit">
              Aggiungi
            </Button>
          </Form>
        </div>
        <Form.Group controlId="formTutorFile" className="mb-3">
          <Form.Label>Carica File CVS Tutor</Form.Label>
          <InfoIconBis text={<img src={pic}></img>}/>
          {/* <InfoIcon text={<img src={pic}></img>} /> */}
          <Form.Control type="file" />
        </Form.Group>
      </div>
    </div>
  );
}

export default TutorManagement;