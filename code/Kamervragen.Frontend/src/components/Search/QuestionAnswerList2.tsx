import React, { useState } from 'react';
import { Checkbox, makeStyles, tokens, Text, Accordion, AccordionHeader, AccordionItem, AccordionPanel, AccordionToggleEventHandler } from '@fluentui/react-components';
import { IDocumentResult, IQuestions_And_Answers } from '../../models/DocumentResult';
import Markdown from 'react-markdown';
import { ISearchDocument } from '../../models/SearchDocument';
import { DataPointsContent } from '../../models/ChatMessage';

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
  document: DataPointsContent;
  onSelectionChange: (selectedPairs: SelectedQAPair[]) => void;
}

export interface SelectedQAPair {
  documentId: string;
  question: string;
  answer: string | undefined;
}

const QuestionAnswerList2: React.FC<QuestionAnswerListProps> = ({ document, onSelectionChange }) => {
  const classes = useStyles();
  const [selectedPairs, setSelectedPairs] = useState<SelectedQAPair[]>([]);
  const [openItems, setOpenItems] = React.useState(["1"]);
  const handleToggle: AccordionToggleEventHandler<string> = (_event, data) => {
    setOpenItems(data.openItems);
  };
  
  const handleCheckboxChange = (qaPair: IQuestions_And_Answers, checked: boolean) => {
    let updatedSelectedPairs = [...selectedPairs];
    if (checked) {
      updatedSelectedPairs.push({
        documentId: document.documentId,
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
      {document.questionsAndAnswers.map((qa, index) => (
        <div key={index} className={classes.qaPair}>
          <Checkbox
            className={classes.checkbox}
            onChange={(e, data) => handleCheckboxChange(qa, data.checked === true)}
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

export default QuestionAnswerList2;