import React from "react";
import styles from './ActiveTutorings.module.css'

import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

export default class ConfirmModal extends React.Component {
  
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
            <this.renderBody selectedList={this.props.selectedList}/>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="warning" onClick={() => this.handleConfirm()}>
              Annulla
            </Button>
            <Button variant="secondary" onClick={() => this.handleConfirm()}>
              Conferma conclusione
            </Button>
          </Modal.Footer>
        </Modal>
      </>
    );
  };

  renderBody(props) {
    if(props.selectedList.length == 0) {
      return(<div>Nessun Tutoraggio Selezionato</div>);
    }

    return(
      <table className={styles.table}>
              <thead>
                <tr>
                  <th scope="col">Cod. Matr. Tutor</th >
                  <th scope="col">Nome Tutor</th >
                  <th scope="col">Cognome Tutor</th >
                  <th scope="col">Cod. Matr. Studente</th >
                  <th scope="col">Codice Esame</th >
                  <th scope="col">Data Inizio</th >
                </tr>
              </thead>
              <tbody>
                {props.selectedList.map((tutoring) =>
                (
                  <tr key={tutoring.id}>
                    <td>{tutoring.tutorNumber}</td>
                    <td>{tutoring.tutorName}</td>
                    <td>{tutoring.tutorSurname}</td>
                    <td>{tutoring.studentNumber}</td>
                    <td>{tutoring.examCode}</td>
                    <td>{tutoring.start_date.toLocaleString()}</td>
                  </tr>
                )
                )}
              </tbody>
            </table>
    );
  }

  handleConfirm() {
    this.props.handleVisibility();
  }
}