import React from "react";
import styles from './ActiveTutorings.module.css'
import configData from "../../config/config.json"

import Button from 'react-bootstrap/Button';
import Form from 'react-bootstrap/Form';
import { makeCall } from "../../MakeCall";
import { Spinner } from "react-bootstrap";


export default class ActiveTutoringsModal extends React.Component
{
  constructor(props)
  {
    super(props);
    this.state = {
      TutoringsList: props.selectedContent,
      DurationList: props.selectedContent.map((tutoring) =>
      {
        return {
          Id: tutoring.Id,
          Duration: 0,
          IsPending: false
        }
      }),
      AlertText: "",
    }
  }

  static getDerivedStateFromProps(props, state)
  {
    if (props.selectedContent !== state.TutoringsList)
    {
      return {
        TutoringsList: props.selectedContent
      };
    }
    return null; // No change to state
  }


  async tryEndingTutorings()
  {
    const temp = this.state.DurationList;
    if (temp.filter((x) => x.Duration === 0).length !== 0)
    {
      // Some tutorings have no attached duration
      this.setState({
        AlertText: "Alcuni tutoraggi non hanno un valore valido nel campo Durata in Ore",
      })
      return;
    }

    this.setState({
      IsPending: true
    })
    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + '/tutoring/end', method: "PUT", hasAuth: true, status: status, body: JSON.stringify(this.state.DurationList) });

    this.setState({
      IsPending: false
    })

    if (status.code !== 200)
    {
      this.setState({
        AlertText: result,
      })
      return;
    }
    this.props.onModalEvent();

    // Hide alert after a positive response
    this.setState({
      AlertText: ""
    });
  }

  changeDuration(index, value)
  {
    const temp = this.state.DurationList;
    if (isNaN(value))
      temp[index].Duration = 0;
    else
      temp[index].Duration = value;
    this.setState({
      DurationList: temp,
    })
  }

  renderContent(props)
  {
    const rows = props.selectedList;
    const durations = props.durations;

    const renderBody = rows.map((tutoring, i) =>
    {
      var formClass = durations[i].Duration === 0 ? styles.formHoursEmpty : styles.formHours;
      const textForm = <Form.Control
        className={formClass}
        type="text"
        placeholder="h"
        onChange={(e) => props.changeDuration(i, parseInt(e.target.value))} />;

      return (
        <tr key={tutoring.Id}>
          <td>{tutoring.TutorCode}</td>
          <td>{tutoring.TutorSurname}</td>
          <td>{tutoring.TutorName}</td>
          <td>{tutoring.StudentCode}</td>
          <td>{tutoring.ExamCode}</td>
          <td>{tutoring.StartDate.toLocaleString()}</td>
          <td className={styles.tdHours}>{textForm}</td>
        </tr>
      );
    });

    if (rows.length !== 0)
    {
      return (
        <>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Cod. Matr. Tutor</th >
                <th scope="col">Cognome Tutor</th >
                <th scope="col">Nome Tutor</th >
                <th scope="col">Cod. Matr. Studente</th >
                <th scope="col">Codice Esame</th >
                <th scope="col">Data Inizio</th >
                <th scope="col">Durata in Ore</th >
              </tr>
            </thead>
            <tbody>
              {renderBody}
            </tbody>
          </table>
          <div>
            <div className={styles.confirmStateChangeDiv}>
              {props.isPending && <div className={styles.pendingCircleModal}><Spinner animation="border" /></div>}
              <Button className={styles.endTutoringsBtn} variant="warning" onClick={() => props.tryEndingTutorings()}>
                Concludi
              </Button>
            </div>
          </div>
        </>
      )
    }
    else
      return (<div>Nessun Tutoraggio Selezionato</div>);
  }

  render()
  {
    return (
      <>
        {
          this.state.AlertText !== "" ?
            <div className={styles.AlertText}>{this.state.AlertText}</div>
            : <></>
        }
        <this.renderContent
          isPending={this.state.IsPending}
          selectedList={this.state.TutoringsList}
          durations={this.state.DurationList}
          tryEndingTutorings={() => this.tryEndingTutorings()}
          changeDuration={(i, value) => this.changeDuration(i, value)}
        />
      </>
    );
  }
}

