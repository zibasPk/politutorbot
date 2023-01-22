
import styles from "./DataManagement.module.css";

import TutorData from "./TutorData";

function DataManagement()
{
  return (
    <>
      <div className={styles.content}>
        <TutorData />
      </div>
    </>
  );
}

export default DataManagement;