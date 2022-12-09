import React from "react";

import configData from "../../config/config.json"

import styles from './Reservations.module.css';
import Table from "../utils/Table";
import ModalBody from "./ModalBody";
import CircularProgress from '@mui/material/CircularProgress';

const Headers = {
  Id: "Prenotazione",
  Tutor: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  Exam: "Codice Esame",
  Student: "Cod. Matr. Studente",
  ReservationTimestamp: "Data"
};

class Reservations extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      ProcessedReservations: undefined,
      PendingReservations: undefined
    };
  }

  // prepare object arrays for tables
  componentDidMount() {
    fetch(configData.botApiUrl + '/reservations', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((reservations) => {
        reservations.map((elem) => {
          if (elem.Exam === null)
            elem.Exam = "OFA"
          elem.ReservationTimestamp = new Date(elem.ReservationTimestamp)
        });

        let processedReservations = reservations.filter((x) => x.IsProcessed)
          .map(({ state, ...key }) => key);
        let pendingReservations = reservations.filter((x) => !x.IsProcessed)
          .map(({ state, ...key }) => key);
        this.setState({
          ProcessedReservations: processedReservations,
          PendingReservations: pendingReservations
        })
      })
  }

  render() {
    return (
      <div className={styles.content}>
        <h1>Prenotazioni da Gestire</h1>

        {this.state.PendingReservations === undefined ? <CircularProgress /> :
          <>
            <Table headers={Headers} content={this.state.PendingReservations} hasChecks={true}
              modalProps={{
                modalContent: ModalBody,
                modalTitle: "Conferma prenotazione selezionate"
              }}
            />
          </>
        }
        <h1>Storico Prenotazioni</h1>
        {this.state.ProcessedReservations === undefined ? <CircularProgress /> :
          <Table headers={Headers} content={this.state.ProcessedReservations} hasChecks={false} />
        }
      </div>
    )
  }
}


export default Reservations;