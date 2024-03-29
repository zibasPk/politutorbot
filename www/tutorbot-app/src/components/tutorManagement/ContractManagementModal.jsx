
import { useState } from "react";

import styles from "./TutorManagement.module.css";
import configData from "../../config/config.json";


import { Form } from "react-bootstrap";
import { Button } from 'react-bootstrap';
import { makeCall } from "../../MakeCall";
import { Spinner } from "react-bootstrap";

function ContractManagementModal(props)
{
  const [changedTutorings, setChangedTutorings] = useState([]);
  const [alert, setAlert] = useState("");
  const [isPending, setIsPending] = useState(false);

  const changeContracts = async () =>
  {
    changedTutorings.forEach(async tutoring =>
    {
      setIsPending(true);
      let status = { code: 0 };
      let result = await makeCall({ url: configData.botApiUrl + '/tutor/' + tutoring.TutorCode + "/contract/" + tutoring.ContractState, method: "PUT", hasAuth: true, status: status });
      setIsPending(false);

      if (status.code !== 200)
      {
        setAlert(result);
        return;
      }
      props.onModalEvent();
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
        <div className={styles.modalBody}>
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
        </div>
        <div>
          <div className={styles.confirmStateChangeDiv}>
            {isPending && <div className={styles.pendingCircleModal}><Spinner animation="border" /></div>}
            <Button className={styles.confirmStateChangeBtn} variant="warning" onClick={() => changeContracts()}>
              Conferma
            </Button>
          </div>
        </div>
      </>
    );
  }
  else
    return (<div>Nessun Tutor Selezionato</div>);

}

export default ContractManagementModal;