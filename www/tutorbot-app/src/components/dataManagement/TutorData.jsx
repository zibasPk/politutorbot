import React, { useEffect, useState } from 'react';

import styles from './DataManagement.module.css'
import configData from "../../config/config.json";

import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

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
        <Collapse in={expanded} timeout="auto" unmountOnExit className={styles.tutorDataCont}>
          <DeleteTutors />
        </Collapse>
      </div>
    </>
  );
}

function DeleteTutors()
{
  const [show, setShow] = useState(false);
  const [modalText, setText] = useState("");
  const [textClass, setTextClass] = useState(styles.deletionAlert);
  const [showButton, setShowButton] = useState(true);

  const handleShow = () =>
  {
    setText("Attenzione! Questa azione é irreversibile sei sicuro di voler continuare?");
    setTextClass(styles.deletionAlert);
    setShow(true);
    setShowButton(true);
  };

  const handleConfirm = () =>
  {
    fetch(configData.botApiUrl + '/tutors/', {
      method: 'DELETE',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp =>
    {
      if (!resp.ok)
        return resp.text();
    })
      .then((text) =>
      {
        if (text !== undefined)
        {
          setText(text);
          return;
        }
        setText("Eliminazione avvenuta con successo");
        setTextClass(styles.successAlert);
        setShowButton(false);
      })
  }

  return (
    <>
      <div>Usa questa funzionalità per eliminare tutti i tutor e tutoraggi dal sistema</div>
      <Button className={styles.btnDeleteTutors} variant="danger" onClick={handleShow}>
        Reset dati Tutor e Tutoraggi
      </Button>
      <Modal show={show} onHide={() => setShow(false)} dialogClassName={styles.deleteTutorModal}>
        <Modal.Header closeButton>
          <Modal.Title>Eliminazione Tutor e Tutoraggi</Modal.Title>
        </Modal.Header>
        <Modal.Body className={textClass}>{modalText}</Modal.Body>
        {
          showButton ? <Modal.Footer>
            <Button variant="danger" onClick={handleConfirm}>
              Conferma
            </Button>
          </Modal.Footer>
            :
            <></>
        }
      </Modal>
    </>);
}
