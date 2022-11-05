import React from "react";
import styles from './ActiveTutorings.module.css'

import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

export default class ConfirmModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      TutoringList: props.selectedList
    }
  }


  render() {
    return (
    <>
      <Modal show={this.props.show}
        onHide={this.props.handleVisibility}
        backdrop="static"
        dialogClassName={styles.myModal}
      >
        <Modal.Header closeButton>
          <Modal.Title>Conferma conclusione tutoraggi selezionati</Modal.Title>
        </Modal.Header>
        <Modal.Body >
          ciao
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => this.handleConfirm()}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    </>
    );
  };

  handleConfirm() {
    this.props.handleVisibility();
  }
}