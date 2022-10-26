import React, { useState } from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import './ModalPopup.css';

export default class ModalPopup extends React.Component {
  handleConfirm() {
    console.log(this.props.selectedList);
    this.props.handleVisibility();
  }

  handleHeaderClick(i) {
    const tempList = this.state.HeaderArrows
    switch (tempList[i]) {
      case 0:
        tempList[i] = -1;
        break;
      case 1:
        tempList[i] = -1;
        break;
      case -1:
        tempList[i] = 1;
        break;
      default:
        console.error("Invalid HeaderCell arrow direction: " + tempList[i]);
        return;
    }
    tempList.forEach((arrow, index) => { if (i !== index) arrow = 0 });
    this.setState({
      HeaderArrows: tempList
    });
    this.sortBy(i);
  }

  renderBody(props) {
    if (props.selectedList.length != 0) {
      const rows = props.selectedList;
      return (
        <table className="table">
          <thead>
            <tr>
              <th scope="col">Prenotazione</th >
              <th scope="col">Cod. Matr. Tutor</th >
              <th scope="col">Nome Tutor</th >
              <th scope="col">Cognome Tutor</th >
              <th scope="col">Codice Esame</th >
              <th scope="col">Cod. Matr. Studente</th >
              <th scope="col">Data</th >
              <th scope="col">Stato</th >
            </tr>
          </thead>
          <tbody>
            {rows.map((reservation) =>
            (
              <tr key={reservation.id}>
                <td>{reservation.id}</td>
                <td>{reservation.tutorNumber}</td>
                <td>{reservation.tutorName}</td>
                <td>{reservation.tutorSurname}</td>
                <td>{reservation.examCode}</td>
                <td>{reservation.studentNumber}</td>
                <td>{reservation.timeStamp}</td>
              </tr>
            )
            )}
          </tbody>
        </table>
      )
    }
    else
      return (<div>Nessuna prenotazione selezionata</div>);
  }

  render() {
    return (
      <Modal show={this.props.show}
        onHide={this.props.handleVisibility}
        backdrop="static"
        dialogClassName="my-modal"
      >
        <Modal.Header closeButton>
          <Modal.Title>Conferma prenotazione selezionate</Modal.Title>
        </Modal.Header>
        <Modal.Body >
          <this.renderBody selectedList={this.props.selectedList}/>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => this.handleConfirm()}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    )
  }
}

