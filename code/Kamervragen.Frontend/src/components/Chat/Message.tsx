import { Field, makeStyles, ProgressBar, tokens, Button, Drawer, DrawerBody, DrawerHeader, DrawerHeaderTitle, useRestoreFocusSource, useRestoreFocusTarget, Caption1, Card, CardHeader } from '@fluentui/react-components';
import { Icon, Pivot, PivotItem, Stack, Text} from '@fluentui/react'
import { Citation, IChatMessage } from '../../models/ChatMessage';
import { parseAnswerToMarkdown } from './AnswerParser';
import { useMemo, useState } from 'react';
import remarkGfm from 'remark-gfm'; // For extended markdown support
import ReactMarkdown from 'react-markdown';
import { Checkmark12Regular, ChevronCircleRight16Regular, ChevronCircleRight24Regular, CopyRegular, Dismiss24Regular, Lightbulb16Regular, MoreHorizontal20Regular } from '@fluentui/react-icons';
import { ThoughtProcess } from './ThoughtProcess';
import DocumentQAViewer from './DocumentQAViewer';
import { SelectedQAPair } from '../Search/QuestionAnswerList';
import DocumentQAViewerLocal from './DocumentQAViewerLocal';

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

    },
    card: {
        width: "100%",
        height: "fit-content",
        marginBottom: tokens.spacingVerticalS
    },
    caption: {
        color: tokens.colorNeutralForeground3,
      },
});

type messageProps = {
    message: IChatMessage;
    onFollowUp: (question: string) => void;
    onQASelected: (selectedQAPairs: SelectedQAPair[]) => void;
    selectedChatId: string | undefined; // Add this line
}

export function Message({ message, selectedChatId, onFollowUp, onQASelected }: messageProps) {
    // const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedCitation, setSelectedCitation] = useState<any>(null);
    const [copied, setCopied] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const [selectedQAPairs, setSelectedQAPairs] = useState<SelectedQAPair[]>([]);
    const [isReferenceOpen, setIsReferenceOpen] = useState(false);
    // Overlay Drawer will handle focus by default, but inline Drawers need manual focus restoration attributes, if applicable
    const restoreFocusSourceAttributes = useRestoreFocusSource();
  
    const onSupportingContentClicked = () => {
        setIsOpen(true);
    }

    
    const classes = useClasses();

    const onQAPairsSelected = (selectedPairs: SelectedQAPair[]) => {
        // Update local state by appending new pairs
        setSelectedQAPairs((selectedQAPairs) => [...selectedQAPairs, ...selectedPairs]);

        // Notify parent component to append the new pairs
        onQASelected(selectedPairs);
    }

    const handleCopy = () => {
        // Single replace to remove all HTML tags to remove the citations
        const textToCopy = message.content.replace(/<a [^>]*><sup>\d+<\/sup><\/a>|<[^>]+>/g, "");

        navigator.clipboard
            .writeText(textToCopy)
            .then(() => {
                setCopied(true);
                setTimeout(() => setCopied(false), 2000);
            })
            .catch(err => console.error("Failed to copy text: ", err));
    };


    function onReferenceClicked(index:number): void {
        if (!message?.context?.dataPointsContent) return;
        {
            setSelectedCitation(message.context.dataPointsContent[index]);
        }
        setIsReferenceOpen(true);
    }

    return (
        <><>
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
                            <Stack horizontal horizontalAlign="end" className={classes.toolbarContainer}>
                                <Button
                                    icon={copied ? <Checkmark12Regular /> : <CopyRegular />}
                                    title="Copy answer"
                                    aria-label="Copy answer"
                                    onClick={handleCopy} />
                                <Button
                                    icon={<Lightbulb16Regular />}
                                    title="Thought Process"
                                    aria-label="ThoughtProcess"
                                    onClick={() => onSupportingContentClicked()} />
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
                        <div className='citations'>
                            {message.context?.dataPointsContent && message.context.dataPointsContent.map((citation, index) => (
                                <Card className={classes.card} size="small" role="listitem">
                                    <CardHeader
                                        header={<Text>{citation.fileName}</Text>}
                                        description={<Caption1 className={classes.caption}>
                                            {citation.onderwerp}
                                        </Caption1>}
                                        action={<Button
                                            onClick={() => onReferenceClicked(index)}
                                            appearance="transparent"
                                            icon={<ChevronCircleRight24Regular />} />} />
                                </Card>
                            ))}
                        </div>
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
                open={isReferenceOpen}
                onOpenChange={(_, { open }) => setIsReferenceOpen(open)}
            >
                <DrawerHeader>
                    <DrawerHeaderTitle
                        action={<Button
                            appearance="subtle"
                            aria-label="Close"
                            icon={<Dismiss24Regular />}
                            onClick={() => setIsReferenceOpen(false)} />}
                    >
                        
                    </DrawerHeaderTitle>
                </DrawerHeader>
                <DrawerBody>
                {selectedCitation && (
                    <DocumentQAViewerLocal
                    document={selectedCitation}
                    chatId={selectedChatId}
                    onQAPairsSelected={onQAPairsSelected} />
                      )}
                </DrawerBody>
            </Drawer></><Drawer
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
                            <Stack horizontalAlign="center">
                                <ThoughtProcess thoughts={message.context?.thoughts || []} />
                            </Stack>
                        </div>
                    </div>
                </DrawerBody>
            </Drawer></>
     
    );
};
