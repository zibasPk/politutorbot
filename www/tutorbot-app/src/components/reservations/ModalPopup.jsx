import React, { useState } from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import './ModalPopup.css';
import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';

export default class ModalPopup extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      ResList: props.selectedList
    }
  }

  static getDerivedStateFromProps(props, state) {
    return{
      ResList: props.selectedList
    };
  }

  handleConfirm() {
    this.props.handleVisibility();
  }

  renderBody(props) {
    const rows = props.resList;
    if (rows.length != 0) {
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
                <MailCell reservation={reservation} />
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
          <MailTemplate/>
          <this.renderBody resList={this.state.ResList} />
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

class MailCell extends React.Component {
  render() {
    return (
      <td className="td-mail">
        <MarkEmailReadIcon className="btn-mail"/>
      </td>
    );
  }
}

function MailTemplate(props) {
  var arrow = expanded ? <KeyboardArrowDownIcon/> : <KeyboardArrowUpIcon/>
  return(
    <>
    <div>Mostra Modello Email <arrow/></div>
    <div>
    Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
    Nulla fringilla tellus sagittis quam eleifend molestie. 
    Cras nisi nulla, bibendum vitae fermentum finibus, gravida non diam. 
    Sed laoreet porttitor luctus. Suspendisse id sem posuere, efficitur ex non, scelerisque est. 
    Aliquam scelerisque luctus elit eget tempus. 
    Aliquam felis felis, sodales et felis. 
    </div>
    </>
  );
}

