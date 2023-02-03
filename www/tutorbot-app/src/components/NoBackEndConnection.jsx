import styles from './NoBackEnd.module.css';
import { Button } from '@mui/material';

export default function ()
{
  const refreshPage = () => {
    window.location.reload(false);
  }

  return (<div className={styles.container}>
  <div className={styles.errorMessage}>
    <i className={styles.errorIcon}>Errore</i>
    <div className={styles.errorMessage}>Connessione al back-end interrotta</div>
    <Button onClick={refreshPage} className={styles.tryAgainButton}>Riprova</Button>
  </div>
</div>);
}
