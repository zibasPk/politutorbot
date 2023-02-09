import React from "react";

import styles from './Reservations.module.css';
import configData from "../../config/config.json";
import RefreshableComponent from "../Interfaces";

import Table from "../utils/Table";
import ModalBody from "./ModalBody";
import CircularProgress from '@mui/material/CircularProgress';
import { makeCall } from "../../MakeCall";

const Headers = {
  Id: "Prenotazione",
  Tutor: "Cod. Matr. Tutor",
  TutorSurname: "Cognome Tutor",
  TutorName: "Nome Tutor",
  Exam: "Codice Esame",
  Student: "Cod. Matr. Studente",
  ReservationTimestamp: "Data"
};

class Reservations extends RefreshableComponent {
  constructor(props) {
    super(props);
    this.state = {
      ProcessedReservations: undefined,
      PendingReservations: undefined
    };
  }

  async refreshData() {

    let status = {code: 0};
    let result = await makeCall({ url: configData.botApiUrl + '/reservations', method: "GET", hasAuth: true, status: status });

    result.map((elem) => {
      if (elem.Exam === null)
        elem.Exam = "OFA"
      elem.ReservationTimestamp = new Date(elem.ReservationTimestamp)
    });

    const processedReservations = result.filter((x) => x.IsProcessed)
      .map(({ state, ...key }) => key);
    const pendingReservations = result.filter((x) => !x.IsProcessed)
      .map(({ state, ...key }) => key);

    this.setState({
      ProcessedReservations: processedReservations,
      PendingReservations: pendingReservations
    });
  }

  render() {
    return (
      <div className={styles.content}>
        <h1>Prenotazioni da Gestire</h1>

        {this.state.PendingReservations === undefined ? <div className='pendingDiv'><CircularProgress /></div> :
          <>
            <Table headers={Headers} content={this.state.PendingReservations} hasChecks={true}
              modalProps={{
                modalContent: ModalBody,
                modalTitle: "Gestisci prenotazioni selezionate",
                onModalEvent: () => this.refreshData()
              }}
              
            />
          </>
        }
        <h1>Storico Prenotazioni</h1>
        {this.state.ProcessedReservations === undefined ? <div className='pendingDiv'><CircularProgress /></div> :
          <Table headers={Headers} content={this.state.ProcessedReservations} hasChecks={false} />
        }
      </div>
    )
  }
}


export default Reservations;