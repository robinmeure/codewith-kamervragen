import React, { useState } from 'react';
import { Checkbox, makeStyles, tokens, Text, Accordion, AccordionHeader, AccordionItem, AccordionPanel, AccordionToggleEventHandler, Tag, TagGroup } from '@fluentui/react-components';
import { IDocumentResult } from '../../models/DocumentResult';
import Markdown from 'react-markdown';

const useStyles = makeStyles({
  container: {
    padding: tokens.spacingVerticalM,
    width: '100%',
  },
  qaPair: {
    display: 'flex',
    alignItems: 'center',
    marginBottom: tokens.spacingVerticalS,
  },
  checkbox: {
    verticalAlign: 'top',
    marginRight: tokens.spacingHorizontalS,
  },
  accordionHeader: {
    backgroundColor: tokens.colorNeutralBackground3Hover,
},
accordionPanel: {
  paddingLeft: '1rem',
},
accordionItem: {
  margin: '1rem',
},
tags: {
  marginTop: '10px',
  display: 'flex',
  flexWrap: 'wrap',
  borderTop: '1px solid #ccc',
  paddingTop: '10px',
},
highlights: {
  flexGrow: 1,
}
});

interface QuestionAnswerListProps {
  document: IDocumentResult;
  onSelectionChange: (selectedPairs: SelectedQAPair[]) => void;
}

export interface SelectedQAPair {
  documentId: string;
  question: string;
  answer: string;
}

const QuestionAnswerList: React.FC<QuestionAnswerListProps> = ({ document, onSelectionChange }) => {
  const classes = useStyles();
  const [selectedPairs, setSelectedPairs] = useState<SelectedQAPair[]>([]);
  const [openItems, setOpenItems] = React.useState(["1"]);
  const handleToggle: AccordionToggleEventHandler<string> = (event, data) => {
    setOpenItems(data.openItems);
  };
  
  const handleCheckboxChange = (qaPair: { documentId:string, question: string; answer: string }, checked: boolean) => {
    let updatedSelectedPairs = [...selectedPairs];
    if (checked) {
      updatedSelectedPairs.push({
        documentId: document.id,
        question: qaPair.question,
        answer: qaPair.answer,
      });
    } else {
      updatedSelectedPairs = updatedSelectedPairs.filter(
        (pair) => pair.question !== qaPair.question
      );
    }
    setSelectedPairs(updatedSelectedPairs);
    onSelectionChange(updatedSelectedPairs);
  };

  return (
    <div className={classes.container}>
      
      <Accordion 
        multiple
        openItems={openItems}
        onToggle={handleToggle}
        collapsible
        > 
      {document.questions_and_answers.map((qa, index) => (
        <div key={index} className={classes.qaPair}>
          <Checkbox
            className={classes.checkbox}
            onChange={(e, data) => handleCheckboxChange(qa, data.checked)}
          />
          <div>
            
            <AccordionItem key={index} value={index} className={classes.accordionItem}>
              <AccordionHeader  className={classes.accordionHeader}>
                <Text weight="semibold">{qa.question}</Text>
              </AccordionHeader>
              <AccordionPanel className={classes.accordionPanel}>
                <Markdown>{qa.answer ? qa.answer : 'No answer provided.'}</Markdown>
              </AccordionPanel>
            </AccordionItem>
          </div>
        </div>
      ))}
      </Accordion>
    </div>
  );
};

export default QuestionAnswerList;