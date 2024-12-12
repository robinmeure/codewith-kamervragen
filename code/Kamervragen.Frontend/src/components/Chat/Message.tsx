import { Field, makeStyles, ProgressBar, tokens, Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, Drawer, DrawerBody, DrawerHeader, DrawerHeaderTitle, useRestoreFocusSource, useRestoreFocusTarget, Divider } from '@fluentui/react-components';
import { FontIcon, IconButton, Pivot, PivotItem, Stack, Text} from '@fluentui/react'
import { ChatAppResponse, Citation, IChatMessage } from '../../models/ChatMessage';
import Markdown from 'react-markdown';
import { parseAnswerToMarkdown } from './AnswerParser';
import { useMemo, useState } from 'react';
import { on } from 'events';
import { link } from 'fs';
import remarkGfm from 'remark-gfm'; // For extended markdown support
import ReactMarkdown from 'react-markdown';
import { useTranslation } from "react-i18next";
import { Checkmark12Regular, ClipboardTaskList16Regular, CopyRegular, Dismiss24Regular, Lightbulb16Filled, Lightbulb16Regular } from '@fluentui/react-icons';
import { ThoughtProcess } from './ThoughtProcess';
import DocumentQAViewer from './DocumentQAViewer';
import { SelectedQAPair } from '../Search/QuestionAnswerList';

const useClasses = makeStyles({
    userContainer: {
        display: 'flex',
        justifyContent: 'flex-end',
        marginTop: tokens.spacingVerticalL
    },
    assistantContainer: {
        display: 'flex',
        justifyContent: 'start',
        marginTop: tokens.spacingVerticalL
    },
    userTextContainer: {
        backgroundColor: tokens.colorNeutralBackground1Hover,
        borderRadius: tokens.borderRadiusXLarge,
        maxWidth: '80%',
        padding: tokens.spacingHorizontalM,
        paddingTop: 0,
        paddingBottom: 0,
        boxShadow: tokens.shadow2
    },
    assistantTextContainer: {
        backgroundColor: tokens.colorNeutralBackground1Pressed,
        borderRadius: tokens.borderRadiusXLarge,
        maxWidth: '80%',
        padding: tokens.spacingHorizontalM,
        paddingTop: 0,
        paddingBottom: 0,
        boxShadow: tokens.shadow2
    },
    supportingContentContainer: {
        backgroundColor: tokens.colorNeutralBackground3,
        borderRadius: tokens.borderRadiusXLarge,
        maxWidth: '80%',
        padding: tokens.spacingHorizontalM,
        marginTop: "20",
        marginBottom: "20px",
        boxShadow: tokens.shadow2
    },
    subheader: {
        marginTop: tokens.spacingVerticalS,
        paddingBottom: tokens.spacingVerticalXS,
        fontWeight: tokens.fontWeightRegular,
        fontSize: tokens.fontSizeBase200,
        color: tokens.colorNeutralForeground3
    },
    thinkingContainer: {
        width: '50%',
        backgroundColor: tokens.colorNeutralBackground1Pressed,
        borderRadius: tokens.borderRadiusXLarge,
        padding: tokens.spacingHorizontalL,
    },
    title: {
        flexGrow: 1,
        fontSize: tokens.fontSizeBase500,
    },
    followUpContainer: {
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-end',
        marginTop: tokens.spacingVerticalS,
        marginLeft: 'auto',
        marginRight: 'auto',
        maxWidth: '80%',
    },
    followUpButton: {
        marginTop: tokens.spacingVerticalXS
    },
    citationLearnMore: {
        color: tokens.colorNeutralForeground3,
        fontSize: tokens.fontSizeBase200
    },
    citation: {
        color: tokens.colorNeutralForeground3
    },
    modalContent: {
        padding: '20px'
    },
    toolbarContainer: {
        marginBottom: tokens.spacingVerticalS,
        marginTop: tokens.spacingVerticalS

    }

});

type messageProps = {
    message: IChatMessage;
    onFollowUp: (question: string) => void;
    onQASelected: (selectedPairs: SelectedQAPair[]) => void;
    selectedChatId: string | undefined; // Add this line
}

export function Message({ message, selectedChatId, onFollowUp, onQASelected }: messageProps) {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedCitation, setSelectedCitation] = useState<Citation | null>(null);
    const [copied, setCopied] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    // Overlay Drawer will handle focus by default, but inline Drawers need manual focus restoration attributes, if applicable
    const restoreFocusTargetAttributes = useRestoreFocusTarget();
    const restoreFocusSourceAttributes = useRestoreFocusSource();
  
    const onSupportingContentClicked = () => {
        setIsOpen(true);
    }

    const handleCitationClick = (citation: Citation) => {
        setSelectedCitation(citation);
        setIsModalOpen(true);
      };
      const parsedAnswer = useMemo(
        () => parseAnswerToMarkdown(message),
        [message]
      );
    
    const classes = useClasses();

    const onQAPairsSelected = (selectedPairs: SelectedQAPair[]) => {
        onQASelected(selectedPairs);
    }

    const handleCopy = () => {
        // Single replace to remove all HTML tags to remove the citations
        const textToCopy = parsedAnswer.replace(/<a [^>]*><sup>\d+<\/sup><\/a>|<[^>]+>/g, "");

        navigator.clipboard
            .writeText(textToCopy)
            .then(() => {
                setCopied(true);
                setTimeout(() => setCopied(false), 2000);
            })
            .catch(err => console.error("Failed to copy text: ", err));
    };

   const uniqueDocumentIds = Array.from(new Set
    (
        (message.context?.dataPointsContent && message.context?.dataPointsContent.map(citation => citation.documentId)) 
        || []
    )
    );

    return (
        <>
            <div id={message.id} className={message.role == "user" ? classes.userContainer : classes.assistantContainer}>
                {message.content == "" ? (
                    <div className={classes.thinkingContainer}>
                        <Field validationMessage="Thinking..." validationState="none">
                            <ProgressBar />
                        </Field>
                    </div>
                ) : (
                    <div className={message.role == "user" ? classes.userTextContainer : classes.assistantTextContainer}>
                        {message.role === "assistant" ? (
                            <Stack horizontal horizontalAlign="end" className={classes.toolbarContainer} >
                                <Button
                                    icon={copied ? <Checkmark12Regular /> : <CopyRegular />}
                                    title="Copy answer"
                                    aria-label="Copy answer"
                                    onClick={handleCopy}
                                />
                                <Button
                                    icon={<Lightbulb16Regular />}
                                    title="Thought Process"
                                    aria-label="ThoughtProcess"
                                    onClick={() => onSupportingContentClicked()}
                                    
                                />
                                {/* <Button
                                    icon={<ClipboardTaskList16Regular />}
                                    title="Show supporting content"
                                    aria-label="Show supporting content"
                                    onClick={() => onSupportingContentClicked()}
                                    disabled={!message.context?.dataPointsContent}
                                /> */}
                            </Stack>
                        ) : null}
                        <ReactMarkdown remarkPlugins={[remarkGfm]}>{message.content}</ReactMarkdown>
                    </div>
                )}
            </div>

            {message.context?.followup_questions && message.context.followup_questions.length > 0 && (
                <div className={classes.followUpContainer}>
                    {message.context.followup_questions.map((question, index) => (
                        <Button key={index} className={classes.followUpButton} onClick={() => onFollowUp(question)}>
                            {question}
                        </Button>
                    ))}
                </div>
            )}

            <Drawer
                {...restoreFocusSourceAttributes}
                type="overlay"
                position='end'
                size='large'

                separator
                open={isOpen}
                onOpenChange={(_, { open }) => setIsOpen(open)}
            >
                    <DrawerHeader>
                        <DrawerHeaderTitle
                            action={<Button
                                appearance="subtle"
                                aria-label="Close"
                                icon={<Dismiss24Regular />}
                                onClick={() => setIsOpen(false)} />}
                        >
                            Supporting content
                        </DrawerHeaderTitle>
                    </DrawerHeader>
                    <DrawerBody>
                        <div>
                            <div className={classes.userContainer}>
                                <Pivot>
                                <PivotItem
                                        itemKey="thoughtProcess"
                                        headerText="Thought Process"
                                        >
                                         <Stack horizontalAlign="center">
                                            <ThoughtProcess thoughts={message.context?.thoughts || []} />
                                        </Stack>
                                    </PivotItem>
                                    <PivotItem
                                        itemKey="questionsAndAnswers"
                                        headerText="Questions and Answers"
                                        >
                                        <Stack horizontalAlign="center">
                                        {uniqueDocumentIds.map((document, index) => (
                                        <div key={index}>
                                            <DocumentQAViewer
                                            documentId={document}
                                            chatId={selectedChatId}
                                            onQAPairsSelected={onQAPairsSelected}
                                            />
                                        </div>
                                        ))}
                                        </Stack>
                                    </PivotItem>
                                    <PivotItem
                                        itemKey="dataPoints"
                                        headerText="Data Points"
                                        >
                                        <Stack horizontalAlign="center">
                                        {message.context?.dataPointsContent && message.context.dataPointsContent.map((citation, index) => (
                                            <div key={index} className={classes.supportingContentContainer}>
                                                <Stack horizontalAlign="center">
                                                <Text className={classes.subheader}>
                                                    {/* FileName {citation.fileName.length > 50 ? citation.fileName.substring(0, 50) + '...' : citation.fileName} */}
                                                </Text>
                                                <Text className={classes.subheader}>
                                                    Page {citation.pageNumber.length > 50 ? citation.pageNumber.substring(0, 50) + '...' : citation.pageNumber}
                                                </Text>
                                                <Text className={classes.subheader}>
                                                    Id {citation.documentId.length > 50 ? citation.documentId.substring(0, 50) + '...' : citation.documentId}
                                                </Text>
                                                </Stack>
                                                <Text key={index} className={classes.citation}>
                                                    {citation.content}
                                                </Text>
                                            </div>
                                        ))}
                                        </Stack>
                                    </PivotItem>
                                </Pivot>
                            </div>
                        </div>
                    </DrawerBody>
            </Drawer></>
     
    );
};
