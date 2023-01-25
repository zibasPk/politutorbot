
import { useState } from "react";

import styles from "./TutorManagement.module.css";
import configData from "../../config/config.json";

import { Form } from "react-bootstrap";
import { Button } from 'react-bootstrap';

function ContractManagementModal(props)
{
  const [changedTutorings, setChangedTutorings] = useState([]);
  const [alert, setAlert] = useState("");

  const changeContracts = () =>
  {
    changedTutorings.forEach(tutoring =>
    {
      fetch(configData.botApiUrl + '/tutor/' + tutoring.TutorCode + "/contract/" + tutoring.ContractState, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Basic ' + btoa(configData.authCredentials),
        }
      }).then(resp =>
      {
        if (!resp.ok)
          return resp.text();
        props.onModalEvent();
      })
        .then((text) =>
        {
          if (text !== undefined)
          {
            setAlert(text);
            return;
          }
        })
    });
  }

  const handleSelectChange = (e, tutoring) =>
  {
    let value = e.target.value.trim();
    setAlert("");
    const changedTutoring = { ...tutoring };

    switch (value)
    {
      case "non inviato":
        changedTutoring.ContractState = 0;
        break;
      case "inviato":
        changedTutoring.ContractState = 1;
        break;
      case "firmato":
        changedTutoring.ContractState = 2;
        break;
      default:
        break;
    }
    setChangedTutorings(prevArray => [...prevArray, changedTutoring]);
  }


  const headerRow = Object.keys(props.contentHeaders).map((key, i) =>
  {
    const cell = <th key={i}>{props.contentHeaders[key]}</th>
    return cell;
  });

  const contentRow = (row) => Object.keys(props.contentHeaders).map((key, i) =>
  {
    if (key === "ContractState")
    {
      const formClass = changedTutorings.filter((tutoring) => tutoring.TutorCode === row.TutorCode).length > 0 ?
        "" : styles.unalteredSelect;
      console.log(formClass);
      return <td key={i}>
        <Form.Select className={formClass} onChange={(e) => handleSelectChange(e, row)} defaultValue={row[key]}>
          <option key={0}>non inviato</option>
          <option key={1}>inviato</option>
          <option key={2}>firmato</option>
        </Form.Select>
      </td>;
    }

    return <td key={i}>{row[key]}</td>;
  });


  if (props.selectedContent.length !== 0) 
  {
    return (
      <>
        <div className={styles.alertText}>{alert}</div>
        <table>
          <thead>
            <tr>
              {headerRow}
            </tr>
          </thead>
          <tbody>
            {props.selectedContent.map((tutoring, i) => <tr key={i}>{contentRow(tutoring)}</tr>)}
          </tbody>
        </table>
        <div>
          <Button className={styles.confirmStateChangeBtn} variant="warning" onClick={() => changeContracts()}>
            Conferma
          </Button>
        </div>
      </>
    );
  }
  else
    return (<div>Nessun Tutor Selezionato</div>);

}

export default ContractManagementModal;