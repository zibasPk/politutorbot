import styles from "./DeleteButton.module.css";
import React, { useState } from 'react';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import { makeCall } from '../../utils/MakeCall';

/**
 * Generalized JSX Component for calling a mass delete endpoint
 * @param {object} props
 * @param {string} props.btnText text displayed on button
 * @param {string} props.modalTitle title of alert Modal
 * @param {string} props.deleteEndpoint end point to call for deletion
 */
export default function DeleteButton(props)
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

  const confirmAction = async () => {
    let status = { code: 0 };
    let result = await makeCall({ url: props.deleteEndpoint, method: 'DELETE', hasAuth: true, status: status });

    if (status.code !== 200)
    {
      if (result == "")
        return;
      setText(result);
      return
    }
    setText("Eliminazione avvenuta con successo");
    setTextClass(styles.successAlert);
    setShowButton(false);
  }

  return (
    <>
      <Button className={styles.btnDelete} variant="danger" onClick={handleShow}>
        {props.btnText}
      </Button>
      <Modal show={show} onHide={() => setShow(false)} dialogClassName={styles.deleteModal}>
        <Modal.Header closeButton>
          <Modal.Title>{props.modalTitle}</Modal.Title>
        </Modal.Header>
        <Modal.Body className={textClass}>{modalText}</Modal.Body>
        {
          showButton ? <Modal.Footer>
            <Button variant="danger" onClick={confirmAction}>
              Conferma
            </Button>
          </Modal.Footer>
            :
            <></>
        }
      </Modal>
    </>
  );
}
